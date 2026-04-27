-- Meglévő táblák törlése ha vannak
DROP TABLE IF EXISTS transactions;
DROP TABLE IF EXISTS visits;
DROP TABLE IF EXISTS customers;
 
-- Táblák létrehozása
CREATE TABLE customers (
    customer_id INT PRIMARY KEY IDENTITY(1,1),
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL
);
 
CREATE TABLE visits (
    visit_id INT PRIMARY KEY IDENTITY(1,1),
    customer_id INT NOT NULL FOREIGN KEY REFERENCES customers(customer_id),
    visit_date DATE NOT NULL
);
 
CREATE TABLE transactions (
    transaction_id INT PRIMARY KEY IDENTITY(1,1),
    visit_id INT NOT NULL FOREIGN KEY REFERENCES visits(visit_id),
    amount DECIMAL(10,2) NOT NULL,
    transaction_date DATE NOT NULL
);