CREATE PROCEDURE dbo.usp_CreateOrder
    @CustomerId INT,
    @Items dbo.OrderItemInput READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrderId INT;

    DECLARE @AggregatedItems TABLE
    (
        product_id INT PRIMARY KEY,
        total_quantity INT NOT NULL
    );

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Ügyfél ellenőrzése
        IF NOT EXISTS
        (
            SELECT 1
            FROM customers
            WHERE customer_id = @CustomerId
        )
        BEGIN
            RAISERROR(N'A megadott ügyfél nem létezik.', 16, 1);
        END

        -- Van-e rendelési tétel
        IF NOT EXISTS
        (
            SELECT 1
            FROM @Items
        )
        BEGIN
            RAISERROR(N'A rendelés nem tartalmaz tételeket.', 16, 1);
        END

        -- Tételek összesítése
        INSERT INTO @AggregatedItems (product_id, total_quantity)
        SELECT
            product_id,
            SUM(quantity) AS total_quantity
        FROM @Items
        GROUP BY product_id;

        -- Létező termékek ellenőrzése
        IF EXISTS
        (
            SELECT 1
            FROM @AggregatedItems ai
            LEFT JOIN products p
                ON p.product_id = ai.product_id
            WHERE p.product_id IS NULL
        )
        BEGIN
            RAISERROR(N'A rendelésben nem létező termék szerepel.', 16, 1);
        END

        -- Készletellenőrzés
        IF EXISTS
        (
            SELECT 1
            FROM @AggregatedItems ai
            JOIN products p
                ON p.product_id = ai.product_id
            WHERE p.stock_quantity < ai.total_quantity
        )
        BEGIN
            RAISERROR(N'Nincs elegendő készlet az egyik vagy több termékből.', 16, 1);
        END

        -- Rendelés létrehozása
        INSERT INTO orders (customer_id, order_date, status)
        VALUES (@CustomerId, GETDATE(), N'Új');

        SET @OrderId = SCOPE_IDENTITY();

        -- Rendelési tételek létrehozása
        INSERT INTO order_items (order_id, product_id, quantity, unit_price)
        SELECT
            @OrderId,
            p.product_id,
            ai.total_quantity,
            p.unit_price
        FROM @AggregatedItems ai
        JOIN products p
            ON p.product_id = ai.product_id;

        -- Készlet frissítése
        UPDATE p
        SET p.stock_quantity = p.stock_quantity - ai.total_quantity
        FROM products p
        JOIN @AggregatedItems ai
            ON ai.product_id = p.product_id;

        COMMIT TRANSACTION;

        SELECT
            @OrderId AS order_id,
            N'A rendelés sikeresen létrejött.' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO