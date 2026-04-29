CREATE TYPE dbo.OrderItemList AS TABLE (
    product_id INT NOT NULL,
    quantity INT NOT NULL
);
GO