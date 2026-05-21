-- ============================================================
-- EMS Database Setup - Departments
-- ============================================================

IF OBJECT_ID('sp_GetAllDepartments', 'P') IS NOT NULL DROP PROCEDURE sp_GetAllDepartments;
IF OBJECT_ID('sp_GetDepartmentById', 'P') IS NOT NULL DROP PROCEDURE sp_GetDepartmentById;
IF OBJECT_ID('sp_InsertDepartment', 'P') IS NOT NULL DROP PROCEDURE sp_InsertDepartment;
IF OBJECT_ID('sp_UpdateDepartment', 'P') IS NOT NULL DROP PROCEDURE sp_UpdateDepartment;
IF OBJECT_ID('sp_DeleteDepartment', 'P') IS NOT NULL DROP PROCEDURE sp_DeleteDepartment;
IF OBJECT_ID('sp_GetEmployeesByDepartmentId', 'P') IS NOT NULL DROP PROCEDURE sp_GetEmployeesByDepartmentId;
GO

-- Create Departments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
BEGIN
    CREATE TABLE Departments (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(255) NULL
    );
END
GO

-- Get all departments
CREATE PROCEDURE sp_GetAllDepartments
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Description FROM Departments ORDER BY Name;
END
GO

-- Get department by ID
CREATE PROCEDURE sp_GetDepartmentById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Description FROM Departments WHERE Id = @Id;
END
GO

-- Insert department
CREATE PROCEDURE sp_InsertDepartment
    @Name        NVARCHAR(100),
    @Description NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Departments (Name, Description) VALUES (@Name, @Description);
END
GO

-- Update department
CREATE PROCEDURE sp_UpdateDepartment
    @Id          INT,
    @Name        NVARCHAR(100),
    @Description NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Departments SET Name = @Name, Description = @Description WHERE Id = @Id;
END
GO

-- Delete department
CREATE PROCEDURE sp_DeleteDepartment
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Departments WHERE Id = @Id;
END
GO

-- Get employees by department
CREATE PROCEDURE sp_GetEmployeesByDepartmentId
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.Id, e.Name, e.Email, e.DepartmentId,
        d.Name AS DepartmentName, e.Salary, e.JoinedDate
    FROM Employees e
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE e.DepartmentId = @DepartmentId;
END
GO
