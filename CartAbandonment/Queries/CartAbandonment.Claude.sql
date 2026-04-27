SELECT
    c.customer_id,
    c.first_name,
    c.last_name,
    COUNT(v.visit_id) AS total_visits,
    SUM(CASE WHEN t.transaction_id IS NULL THEN 1 ELSE 0 END) AS no_transaction_visits
FROM customers c
INNER JOIN visits v ON v.customer_id = c.customer_id
LEFT JOIN transactions t ON t.visit_id = v.visit_id
WHERE v.visit_date >= '2025-01-01' AND v.visit_date < '2026-01-01'
GROUP BY c.customer_id, c.first_name, c.last_name
HAVING SUM(CASE WHEN t.transaction_id IS NULL THEN 1 ELSE 0 END) > 0
ORDER BY no_transaction_visits DESC;