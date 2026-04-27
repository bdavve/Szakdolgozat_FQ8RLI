CREATE TABLE transactions (
    transaction_id INT PRIMARY KEY IDENTITY(1,1),
    visit_id INT NOT NULL FOREIGN KEY REFERENCES visits(visit_id),
    amount DECIMAL(10,2) NOT NULL,
    transaction_date DATE NOT NULL
);