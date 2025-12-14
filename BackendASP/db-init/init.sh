#!/bin/bash

# Script to initialize PostgreSQL database for AppLearningEnglish

set -e

echo "Initializing AppLearningEnglish database..."

# Create extensions if needed
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Enable UUID extension
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    
    -- Enable pgcrypto for password hashing
    CREATE EXTENSION IF NOT EXISTS "pgcrypto";
    
    -- Enable pg_trgm for full-text search
    CREATE EXTENSION IF NOT EXISTS "pg_trgm";
    
    -- Set timezone
    SET timezone = 'UTC';
    
    -- Grant privileges
    GRANT ALL PRIVILEGES ON DATABASE "$POSTGRES_DB" TO "$POSTGRES_USER";
EOSQL

echo "Database initialization completed successfully!"
