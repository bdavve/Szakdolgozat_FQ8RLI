CREATE PROCEDURE dbo.usp_ProcessNewOrder
    @CustomerID INT,
    @OrderItems NVARCHAR(MAX)  -- JSON: [{"product_id": 1, "quantity": 2}, ...]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Változók
    DECLARE @NewOrderID INT;
    DECLARE @Now DATETIME = GETDATE();

    -- Bemeneti paraméterek validálása
    IF NOT EXISTS (SELECT 1 FROM customers WHERE customer_id = @CustomerID)
    BEGIN
        RAISERROR('Az ügyfél (ID: %d) nem található.', 16, 1, @CustomerID);
        RETURN;
    END

    IF ISJSON(@OrderItems) = 0
    BEGIN
        RAISERROR('Érvénytelen JSON formátum az @OrderItems paraméterben.', 16, 1);
        RETURN;
    END

    -- Rendelési tételek ideiglenes táblába olvasása
    CREATE TABLE #Items (
        product_id  INT,
        quantity    INT
    );

    INSERT INTO #Items (product_id, quantity)
    SELECT
        JSON_VALUE(j.[value], '$.product_id'),
        JSON_VALUE(j.[value], '$.quantity')
    FROM OPENJSON(@OrderItems) AS j;

    -- Validálás: létezik-e minden termék?
    IF EXISTS (
        SELECT 1
        FROM #Items i
        LEFT JOIN products p ON p.product_id = i.product_id
        WHERE p.product_id IS NULL
    )
    BEGIN
        RAISERROR('Egy vagy több termék nem található a katalógusban.', 16, 1);
        RETURN;
    END

    -- Validálás: van-e elég készlet?
    IF EXISTS (
        SELECT 1
        FROM #Items i
        INNER JOIN products p ON p.product_id = i.product_id
        WHERE p.stock_quantity < i.quantity
    )
    BEGIN
        RAISERROR('Egy vagy több termékből nincs elegendő készlet.', 16, 1);
        RETURN;
    END

    -- ========== TRANZAKCIÓ INDÍTÁSA ==========
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1) Rendelés létrehozása
        INSERT INTO orders (customer_id, order_date, status)
        VALUES (@CustomerID, @Now, N'New');

        SET @NewOrderID = SCOPE_IDENTITY();

        -- 2) Rendelési tételek hozzáadása (aktuális egységárral)
        INSERT INTO order_items (order_id, product_id, quantity, unit_price)
        SELECT
            @NewOrderID,
            i.product_id,
            i.quantity,
            p.unit_price
        FROM #Items i
        INNER JOIN products p ON p.product_id = i.product_id;

        -- 3) Készlet csökkentése
        UPDATE p
        SET p.stock_quantity = p.stock_quantity - i.quantity
        FROM products p
        INNER JOIN #Items i ON i.product_id = p.product_id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

    -- Eredmény visszaadása
    SELECT
        @NewOrderID     AS order_id,
        @CustomerID     AS customer_id,
        @Now            AS order_date,
        N'New'          AS status;

    DROP TABLE IF EXISTS #Items;
END;