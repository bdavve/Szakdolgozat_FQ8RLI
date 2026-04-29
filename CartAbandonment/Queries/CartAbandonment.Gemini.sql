WITH VisitStats AS (
    -- 1. lépés: Látogatások és tranzakciók összekapcsolása LEFT JOIN-nal
    SELECT 
        v.customer_id,
        -- DISTINCT biztosítja, hogy ha egy látogatáshoz több tranzakció van, 
        -- akkor is csak egyszer számoljuk a látogatást.
        COUNT(DISTINCT v.visit_id) AS total_visits_2025,
        
        -- Csak azokat a látogatásokat számoljuk, ahol nem volt tranzakció (t.transaction_id IS NULL)
        COUNT(DISTINCT CASE 
            WHEN t.transaction_id IS NULL THEN v.visit_id 
        END) AS visits_without_transactions
    FROM visits v
    LEFT JOIN transactions t ON v.visit_id = t.visit_id
    WHERE v.visit_date >= '2025-01-01' AND v.visit_date < '2026-01-01'
    GROUP BY v.customer_id
)
-- 2. lépés: Ügyféladatok hozzákapcsolása és szűrés
SELECT 
    c.customer_id,
    c.first_name,
    c.last_name,
    vs.total_visits_2025,
    vs.visits_without_transactions
FROM customers c
JOIN VisitStats vs ON c.customer_id = vs.customer_id
WHERE vs.visits_without_transactions > 0
ORDER BY vs.visits_without_transactions DESC;