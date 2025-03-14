
CREATE DATABASE banking_transaction_logs;


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

CREATE TABLE IF NOT EXISTS accounts (
                                        id BIGSERIAL PRIMARY KEY,
                                        user_id BIGINT NOT NULL,
                                        account_type TEXT NOT NULL,
                                        account_number TEXT NOT NULL,
                                        current_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                                        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for faster lookups
CREATE INDEX IF NOT EXISTS idx_accounts_user_id ON accounts(user_id);
CREATE INDEX IF NOT EXISTS idx_accounts_account_number ON accounts(account_number);

-- Add some sample data if needed
INSERT INTO accounts (user_id, account_type, account_number, current_balance)
VALUES
    (1, 'Checking', 'CHK1001', 2500.00),
    (1, 'Savings', 'SAV1001', 10000.00),
    (2, 'Checking', 'CHK1002', 1500.00),
    (2, 'Savings', 'SAV1002', 5000.00)
ON CONFLICT DO NOTHING;

-- Add some sample transaction logs if needed
INSERT INTO transaction_logs (account_id, transaction_type, amount, timestamp, status, details)
VALUES
    (1, 'Deposit', 1000.00, CURRENT_TIMESTAMP - INTERVAL '10 days', 'Completed', 'Salary deposit'),
    (1, 'Withdrawal', 200.00, CURRENT_TIMESTAMP - INTERVAL '7 days', 'Completed', 'ATM withdrawal'),
    (1, 'Deposit', 500.00, CURRENT_TIMESTAMP - INTERVAL '5 days', 'Completed', 'Transfer from savings'),
    (2, 'Deposit', 5000.00, CURRENT_TIMESTAMP - INTERVAL '30 days', 'Completed', 'Initial deposit'),
    (2, 'Deposit', 5000.00, CURRENT_TIMESTAMP - INTERVAL '15 days', 'Completed', 'Additional savings'),
    (2, 'Withdrawal', 500.00, CURRENT_TIMESTAMP - INTERVAL '5 days', 'Completed', 'Transfer to checking'),
    (3, 'Deposit', 1000.00, CURRENT_TIMESTAMP - INTERVAL '10 days', 'Completed', 'Salary deposit')
