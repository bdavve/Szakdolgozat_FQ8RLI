CREATE TABLE visits (
    visit_id INT PRIMARY KEY IDENTITY(1,1),
    customer_id INT NOT NULL FOREIGN KEY REFERENCES customers(customer_id),
    visit_date DATE NOT NULL
);