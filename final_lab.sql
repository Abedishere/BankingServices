-- Create the database if it doesn't exist
-- Note: This should be run as a PostgreSQL superuser before connecting to the specific database

-- Connect to postgres database first to create our new database
\c postgres

-- Drop the database if it exists and create a new one
DROP DATABASE IF EXISTS banking_transaction_logs;
CREATE DATABASE banking_transaction_logs;

-- Connect to our new database
\c banking_transaction_logs

-- Create the transaction_logs table
CREATE TABLE IF NOT EXISTS transaction_logs (
                                                id BIGSERIAL PRIMARY KEY,
                                                account_id BIGINT NOT NULL,
                                                transaction_type TEXT NOT NULL,
                                                amount DECIMAL(18,2) NOT NULL,
    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status TEXT NOT NULL,
    details TEXT
    );

-- Create an index on account_id for faster lookups
CREATE INDEX idx_transaction_logs_account_id ON transaction_logs(account_id);

-- Create an index on timestamp for faster date-based queries
CREATE INDEX idx_transaction_logs_timestamp ON transaction_logs(timestamp);

-- Optional: Add some sample data
-- INSERT INTO transaction_logs (account_id, transaction_type, amount, status, details)
-- VALUES
--     (1001, 'Deposit', 500.00, 'Completed', 'Initial deposit'),
--     (1001, 'Withdrawal', 100.00, 'Completed', 'ATM withdrawal'),
--     (1002, 'Deposit', 1000.00, 'Completed', 'Salary deposit'),
--     (1002, 'Withdrawal', 200.00, 'Completed', 'Online transfer');

-- Grant privileges to the application user (if different from postgres)
-- GRANT ALL PRIVILEGES ON DATABASE banking_transaction_logs TO yourappuser;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO yourappuser;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO yourappuser;