-- Shivohum Construction Expense Tracker
-- LocalDB (used by the API on this PC):
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i 01_CreateDatabase.sql
-- Full SQL Server in SSMS (after your Windows login is mapped):
--   sqlcmd -S localhost -E -i 01_CreateDatabase.sql
-- Connect SSMS to: (localdb)\MSSQLLocalDB

IF DB_ID(N'ShivohumExpenseTracker') IS NULL
BEGIN
    CREATE DATABASE ShivohumExpenseTracker;
END
GO

USE ShivohumExpenseTracker;
GO

IF OBJECT_ID(N'dbo.Transactions', N'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID(N'dbo.ClientPayments', N'U') IS NOT NULL DROP TABLE dbo.ClientPayments;
IF OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL DROP TABLE dbo.Documents;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Projects', N'U') IS NOT NULL DROP TABLE dbo.Projects;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.PaymentModes', N'U') IS NOT NULL DROP TABLE dbo.PaymentModes;
IF OBJECT_ID(N'dbo.Accounts', N'U') IS NOT NULL DROP TABLE dbo.Accounts;
GO

CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100) NOT NULL,
    Username        NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255) NOT NULL,
    Role            NVARCHAR(20)  NOT NULL CONSTRAINT DF_Users_Role DEFAULT ('Staff'),
    IsActive        BIT           NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedOn       DATETIME2     NOT NULL CONSTRAINT DF_Users_CreatedOn DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Staff'))
);

CREATE TABLE dbo.Projects (
    ProjectId       INT IDENTITY(1,1) PRIMARY KEY,
    ProjectName     NVARCHAR(150) NOT NULL,
    ClientName      NVARCHAR(150) NULL,
    Status          NVARCHAR(20)  NOT NULL CONSTRAINT DF_Projects_Status DEFAULT (N'Active'),
    StartDate       DATE          NULL,
    ContractAmount  DECIMAL(18,2) NULL,
    Notes           NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Projects_IsActive DEFAULT (1),
    CreatedOn       DATETIME2     NOT NULL CONSTRAINT DF_Projects_CreatedOn DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_Projects_Status CHECK (Status IN (N'Active', N'Completed', N'OnHold')),
    CONSTRAINT UQ_Projects_Name UNIQUE (ProjectName)
);

CREATE TABLE dbo.Categories (
    CategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName    NVARCHAR(100) NOT NULL,
    Type            NVARCHAR(20)  NOT NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1),
    CONSTRAINT CK_Categories_Type CHECK (Type IN (N'Income', N'Expense')),
    CONSTRAINT UQ_Categories_Name_Type UNIQUE (CategoryName, Type)
);

CREATE TABLE dbo.PaymentModes (
    PaymentModeId   INT IDENTITY(1,1) PRIMARY KEY,
    PaymentModeName NVARCHAR(50)  NOT NULL UNIQUE,
    IsActive        BIT           NOT NULL CONSTRAINT DF_PaymentModes_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Accounts (
    AccountId       INT IDENTITY(1,1) PRIMARY KEY,
    AccountName     NVARCHAR(100) NOT NULL UNIQUE,
    AccountType     NVARCHAR(30)  NOT NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Accounts_IsActive DEFAULT (1),
    CONSTRAINT CK_Accounts_Type CHECK (AccountType IN (N'Cash', N'Bank'))
);

CREATE TABLE dbo.Transactions (
    TransactionId   INT IDENTITY(1,1) PRIMARY KEY,
    TransactionDate DATE          NOT NULL,
    Type            NVARCHAR(20)  NOT NULL,
    CategoryId      INT           NOT NULL,
    Description     NVARCHAR(300) NOT NULL,
    ProjectId       INT           NULL,
    PaymentModeId   INT           NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    AccountId       INT           NULL,
    Notes           NVARCHAR(500) NULL,
    CreatedBy       INT           NOT NULL,
    CreatedOn       DATETIME2     NOT NULL CONSTRAINT DF_Transactions_CreatedOn DEFAULT (SYSUTCDATETIME()),
    UpdatedOn       DATETIME2     NULL,
    CONSTRAINT CK_Transactions_Type CHECK (Type IN (N'Income', N'Expense')),
    CONSTRAINT CK_Transactions_Amount CHECK (Amount > 0),
    CONSTRAINT FK_Transactions_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (CategoryId),
    CONSTRAINT FK_Transactions_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId),
    CONSTRAINT FK_Transactions_PaymentMode FOREIGN KEY (PaymentModeId) REFERENCES dbo.PaymentModes (PaymentModeId),
    CONSTRAINT FK_Transactions_Account FOREIGN KEY (AccountId) REFERENCES dbo.Accounts (AccountId),
    CONSTRAINT FK_Transactions_User FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId)
);

CREATE INDEX IX_Transactions_Date ON dbo.Transactions (TransactionDate);
CREATE INDEX IX_Transactions_Project ON dbo.Transactions (ProjectId);
CREATE INDEX IX_Transactions_Type ON dbo.Transactions (Type);
CREATE INDEX IX_Transactions_Category ON dbo.Transactions (CategoryId);
GO

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
GO

CREATE TABLE dbo.Documents (
    DocumentId  INT IDENTITY(1,1) PRIMARY KEY,
    Kind        NVARCHAR(20)   NOT NULL,
    Title       NVARCHAR(200)  NOT NULL,
    ProjectId   INT            NULL,
    PayloadJson NVARCHAR(MAX)  NOT NULL,
    CreatedBy   INT            NOT NULL,
    CreatedOn   DATETIME2      NOT NULL CONSTRAINT DF_Documents_CreatedOn DEFAULT (SYSUTCDATETIME()),
    UpdatedOn   DATETIME2      NULL,
    CONSTRAINT CK_Documents_Kind CHECK (Kind IN (N'Quotation', N'Invoice')),
    CONSTRAINT FK_Documents_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId),
    CONSTRAINT FK_Documents_User FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId)
);

CREATE INDEX IX_Documents_Kind ON dbo.Documents (Kind);
GO
