-- Existing databases: add project contract amount + client payment history.
-- sqlcmd -S "(localdb)\MSSQLLocalDB" -d ShivohumExpenseTracker -E -i 03_ProjectBudgetAndClientPayments.sql

USE ShivohumExpenseTracker;
GO

IF COL_LENGTH('dbo.Projects', 'ContractAmount') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD ContractAmount DECIMAL(18,2) NULL;
END
GO

IF OBJECT_ID(N'dbo.ClientPayments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClientPayments (
        ClientPaymentId INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId       INT           NOT NULL,
        PaymentDate     DATE          NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        PaymentModeId   INT           NOT NULL,
        AccountId       INT           NULL,
        ReferenceNo     NVARCHAR(100) NULL,
        Notes           NVARCHAR(500) NULL,
        CreatedBy       INT           NOT NULL,
        CreatedOn       DATETIME2     NOT NULL CONSTRAINT DF_ClientPayments_CreatedOn DEFAULT (SYSUTCDATETIME()),
        UpdatedOn       DATETIME2     NULL,
        CONSTRAINT CK_ClientPayments_Amount CHECK (Amount > 0),
        CONSTRAINT FK_ClientPayments_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId),
        CONSTRAINT FK_ClientPayments_PaymentMode FOREIGN KEY (PaymentModeId) REFERENCES dbo.PaymentModes (PaymentModeId),
        CONSTRAINT FK_ClientPayments_Account FOREIGN KEY (AccountId) REFERENCES dbo.Accounts (AccountId),
        CONSTRAINT FK_ClientPayments_User FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId)
    );

    CREATE INDEX IX_ClientPayments_Project ON dbo.ClientPayments (ProjectId);
    CREATE INDEX IX_ClientPayments_Date ON dbo.ClientPayments (PaymentDate);
END
GO
