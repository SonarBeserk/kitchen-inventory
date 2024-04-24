CREATE TABLE IF NOT EXISTS products (
    product_id TEXT PRIMARY KEY NOT NULL,
    brand TEXT NOT NULL,
    name TEXT NOT NULL,
    expiry NUMERIC,
    expiry_type INTEGER,
    perishable BOOLEAN NOT NULL
)