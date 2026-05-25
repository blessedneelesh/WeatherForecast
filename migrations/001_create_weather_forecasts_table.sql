-- Creates table only if it doesn't already exist
CREATE TABLE IF NOT EXISTS weather_forecasts (
    id            SERIAL PRIMARY KEY,
    date          DATE          NOT NULL,
    temperature_c INTEGER       NOT NULL,
    summary       VARCHAR(256),
    location      VARCHAR(128),
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);