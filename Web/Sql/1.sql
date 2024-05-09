CREATE TABLE IF NOT EXISTS products (
    product_id TEXT PRIMARY KEY NOT NULL,
    brand TEXT NOT NULL,
    name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS locations (
    location_id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS inventory(
    inventory_id TEXT PRIMARY KEY NOT NULL,
    product_id TEXT,
    expiry NUMERIC,
    expiry_type INTEGER,
    perishable BOOLEAN NOT NULL,
    amount NUMERIC,
    location_id TEXT,
    FOREIGN KEY (product_id) REFERENCES products(product_id),
    FOREIGN KEY (location_id) REFERENCES locations(location_id)
);