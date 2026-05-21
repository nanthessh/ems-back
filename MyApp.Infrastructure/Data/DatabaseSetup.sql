-- ============================================================
-- EMS Database Setup - Employees
-- ============================================================

-- Drop existing SPs if they exist (for re-run safety)
IF OBJECT_ID('sp_GetAllEmployees', 'P') IS NOT NULL DROP PROCEDURE sp_GetAllEmployees;
IF OBJECT_ID('sp_GetEmployeeById', 'P') IS NOT NULL DROP PROCEDURE sp_GetEmployeeById;
IF OBJECT_ID('sp_InsertEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_InsertEmployee;
IF OBJECT_ID('sp_UpdateEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_UpdateEmployee;
IF OBJECT_ID('sp_DeleteEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_DeleteEmployee;
IF OBJECT_ID('sp_GetDashboardStats', 'P') IS NOT NULL DROP PROCEDURE sp_GetDashboardStats;
GO

-- Create Employees table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Employees' AND xtype='U')
BEGIN
    CREATE TABLE Employees (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100)   NOT NULL,
        Email       NVARCHAR(100)   NOT NULL UNIQUE,
        DepartmentId INT            NOT NULL,
        Salary      DECIMAL(10,2)   NOT NULL,
        JoinedDate  DATE            NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
    );
END
GO

-- Get all employees with department name
CREATE PROCEDURE sp_GetAllEmployees
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.Id,
        e.Name,
        e.Email,
        e.DepartmentId,
        d.Name AS DepartmentName,
        e.Salary,
        e.JoinedDate
    FROM Employees e
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    ORDER BY e.Id DESC;
END
GO

-- Get employee by ID
CREATE PROCEDURE sp_GetEmployeeById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.Id,
        e.Name,
        e.Email,
        e.DepartmentId,
        d.Name AS DepartmentName,
        e.Salary,
        e.JoinedDate
    FROM Employees e
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE e.Id = @Id;
END
GO

-- Insert employee
CREATE PROCEDURE sp_InsertEmployee
    @Name        NVARCHAR(100),
    @Email       NVARCHAR(100),
    @DepartmentId INT,
    @Salary      DECIMAL(10,2),
    @JoinedDate  DATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Employees (Name, Email, DepartmentId, Salary, JoinedDate)
    VALUES (@Name, @Email, @DepartmentId, @Salary, @JoinedDate);
END
GO

-- Update employee
CREATE PROCEDURE sp_UpdateEmployee
    @Id          INT,
    @Name        NVARCHAR(100),
    @Email       NVARCHAR(100),
    @DepartmentId INT,
    @Salary      DECIMAL(10,2),
    @JoinedDate  DATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Employees
    SET Name = @Name,
        Email = @Email,
        DepartmentId = @DepartmentId,
        Salary = @Salary,
        JoinedDate = @JoinedDate
    WHERE Id = @Id;
END
GO

-- Delete employee
CREATE PROCEDURE sp_DeleteEmployee
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Employees WHERE Id = @Id;
END
GO

-- Dashboard stats
CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM Employees)                                            AS TotalEmployees,
        (SELECT COUNT(*) FROM Departments)                                          AS TotalDepartments,
        (SELECT ISNULL(SUM(Salary), 0) FROM Employees)                             AS TotalSalary,
        (SELECT COUNT(*) FROM Employees
         WHERE MONTH(@JoinedDate) = MONTH(GETDATE())
           AND YEAR(@JoinedDate)  = YEAR(GETDATE()))                                AS NewEmployeesThisMonth;
END
GO
