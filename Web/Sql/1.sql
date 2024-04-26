CREATE TABLE IF NOT EXISTS products (
    product_id TEXT PRIMARY KEY NOT NULL,
    brand TEXT NOT NULL,
    name TEXT NOT NULL,
    expiry NUMERIC,
    expiry_type INTEGER,
    perishable BOOLEAN NOT NULL,
    location_id TEXT NOT NULL,
    FOREIGN KEY (location_id) REFERENCES locations(location_id)
);

CREATE TABLE IF NOT EXISTS locations (
    location_id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    description TEXT
);