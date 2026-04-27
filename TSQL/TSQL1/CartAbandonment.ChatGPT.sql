WITH visit_status AS (
    SELECT
        v.visit_id,
        v.customer_id,
        CASE
            WHEN EXISTS (
                SELECT 1
                FROM transactions t
                WHERE t.visit_id = v.visit_id
            ) THEN 1
            ELSE 0
        END AS has_transaction
    FROM visits v
    WHERE v.visit_date >= '2025-01-01'
      AND v.visit_date < '2026-01-01'
)
SELECT
    c.customer_id,
    c.first_name,
    c.last_name,
    COUNT(vs.visit_id) AS total_visits_2025,
    SUM(CASE WHEN vs.has_transaction = 0 THEN 1 ELSE 0 END) AS visits_without_transaction
FROM customers c
INNER JOIN visit_status vs
    ON c.customer_id = vs.customer_id
GROUP BY
    c.customer_id,
    c.first_name,
    c.last_name
HAVING SUM(CASE WHEN vs.has_transaction = 0 THEN 1 ELSE 0 END) > 0
ORDER BY visits_without_transaction DESC;