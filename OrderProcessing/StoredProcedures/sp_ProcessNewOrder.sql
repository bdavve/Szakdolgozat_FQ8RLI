CREATE PROCEDURE dbo.sp_ProcessNewOrder
    @customer_id INT,
    @order_items dbo.OrderItemList READONLY
AS
BEGIN
    -- Biztosítja, hogy hiba esetén a tranzakció azonnal megszakadjon
    SET XACT_ABORT ON;
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Készlet ellenőrzése
        DECLARE @MissingProduct INT;
        
        SELECT TOP 1 @MissingProduct = oi.product_id
        FROM @order_items oi
        JOIN dbo.products p ON oi.product_id = p.product_id
        WHERE p.stock_quantity < oi.quantity;

        IF @MissingProduct IS NOT NULL
        BEGIN
            -- Fontos a pontosvessző a THROW előtt!
            ;THROW 50001, 'Hiba: Nincs elegendő készlet az egyik megrendelt termékből.', 1;
        END

        -- 2. Rendelés létrehozása
        DECLARE @new_order_id INT;

        INSERT INTO dbo.orders (customer_id, order_date, status)
        VALUES (@customer_id, GETDATE(), 'Pending');

        SET @new_order_id = SCOPE_IDENTITY();

        -- 3. Tételek rögzítése (az aktuális árakkal a termék táblából)
        INSERT INTO dbo.order_items (order_id, product_id, quantity, unit_price)
        SELECT 
            @new_order_id,
            oi.product_id,
            oi.quantity,
            p.unit_price
        FROM @order_items oi
        JOIN dbo.products p ON oi.product_id = p.product_id;

        -- 4. Készlet csökkentése
        UPDATE p
        SET p.stock_quantity = p.stock_quantity - oi.quantity
        FROM dbo.products p
        JOIN @order_items oi ON p.product_id = oi.product_id;

        -- Minden sikerült, véglegesítés
        COMMIT TRANSACTION;
        
        PRINT 'A rendelés sikeresen rögzítve. Rendelésszám: ' + CAST(@new_order_id AS VARCHAR(10));

    END TRY
    BEGIN CATCH
        -- Hiba esetén visszagörgetés
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        -- A hiba továbbküldése az alkalmazás felé
        ;THROW;
    END CATCH
END
GO