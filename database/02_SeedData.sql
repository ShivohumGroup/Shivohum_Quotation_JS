USE ShivohumExpenseTracker;
GO

-- Categories (Income)
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'CLIENT PAYMENT' AND Type = N'Income')
    INSERT INTO dbo.Categories (CategoryName, Type) VALUES (N'CLIENT PAYMENT', N'Income');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Professional Fee' AND Type = N'Income')
    INSERT INTO dbo.Categories (CategoryName, Type) VALUES (N'Professional Fee', N'Income');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Other Income' AND Type = N'Income')
    INSERT INTO dbo.Categories (CategoryName, Type) VALUES (N'Other Income', N'Income');

-- Categories (Expense)
INSERT INTO dbo.Categories (CategoryName, Type)
SELECT v.CategoryName, v.Type
FROM (VALUES
    (N'Material', N'Expense'),
    (N'Labour', N'Expense'),
    (N'Software', N'Expense'),
    (N'Office Rent', N'Expense'),
    (N'Electricity', N'Expense'),
    (N'Internet', N'Expense'),
    (N'Travel', N'Expense'),
    (N'Marketing', N'Expense'),
    (N'Printing', N'Expense'),
    (N'Professional Fee', N'Expense'),
    (N'Equipment', N'Expense'),
    (N'Instrument', N'Expense'),
    (N'Construction Expense', N'Expense'),
    (N'Other', N'Expense'),
    (N'Other Expense', N'Expense')
) AS v (CategoryName, Type)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Categories c
    WHERE c.CategoryName = v.CategoryName AND c.Type = v.Type
);

-- Payment modes
INSERT INTO dbo.PaymentModes (PaymentModeName)
SELECT v.Name
FROM (VALUES (N'UPI'), (N'Cash'), (N'Bank Transfer'), (N'Cheque')) AS v (Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PaymentModes p WHERE p.PaymentModeName = v.Name);

-- Accounts / funds (replaces free-text notes like "Shivohum Account")
INSERT INTO dbo.Accounts (AccountName, AccountType)
SELECT v.Name, v.Type
FROM (VALUES
    (N'Personal Cash', N'Cash'),
    (N'Personal Account', N'Bank'),
    (N'Shivohum Account', N'Bank'),
    (N'Avadh Account (Company Fund)', N'Bank')
) AS v (Name, Type)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Accounts a WHERE a.AccountName = v.Name);

-- Construction projects from the Excel tracker
INSERT INTO dbo.Projects (ProjectName, ClientName, Status, StartDate)
SELECT v.ProjectName, v.ClientName, N'Active', CAST(N'2026-08-01' AS DATE)
FROM (VALUES
    (N'AMARBHAI''S HOUSE', N'Amarbhai'),
    (N'TARLESHBHAI''S HOUSE', N'Tarleshabhai'),
    (N'SHOWROOM INTERIOR DESIGN', N'Showroom'),
    (N'VIJAPUR BRICKWORK', N'Vijapur')
) AS v (ProjectName, ClientName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Projects p WHERE p.ProjectName = v.ProjectName);
GO
