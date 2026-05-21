-- ============================================================
-- EMS Database Setup - Auth
-- ============================================================

IF OBJECT_ID('sp_RegisterUser', 'P') IS NOT NULL DROP PROCEDURE sp_RegisterUser;
IF OBJECT_ID('sp_LoginUser', 'P') IS NOT NULL DROP PROCEDURE sp_LoginUser;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        Username     NVARCHAR(100)  NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256)  NOT NULL,
        Role         NVARCHAR(50)   NOT NULL DEFAULT 'User',
        CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Register user (prevents duplicate usernames)
CREATE PROCEDURE sp_RegisterUser
    @Username     NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @Role         NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        RAISERROR('Username already exists.', 16, 1);
        RETURN;
    END
    INSERT INTO Users (Username, PasswordHash, Role)
    VALUES (@Username, @PasswordHash, @Role);
END
GO

-- Login user
CREATE PROCEDURE sp_LoginUser
    @Username     NVARCHAR(100),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Username, Role
    FROM Users
    WHERE Username = @Username AND PasswordHash = @PasswordHash;
END
GO
