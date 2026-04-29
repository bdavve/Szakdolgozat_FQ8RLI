-- 1) Táblatípus a rendelési tételek átadásához
CREATE TYPE dbo.OrderItemInput AS TABLE
(
    product_id INT NOT NULL,
    quantity   INT NOT NULL CHECK (quantity > 0)
);
GO