-- ============================================================
-- EMS - Payroll Module
-- ============================================================

IF OBJECT_ID('sp_GetAllPayroll',        'P') IS NOT NULL DROP PROCEDURE sp_GetAllPayroll;
IF OBJECT_ID('sp_GetPayrollByEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_GetPayrollByEmployee;
IF OBJECT_ID('sp_GeneratePayroll',      'P') IS NOT NULL DROP PROCEDURE sp_GeneratePayroll;
IF OBJECT_ID('sp_UpdatePayrollStatus',  'P') IS NOT NULL DROP PROCEDURE sp_UpdatePayrollStatus;
IF OBJECT_ID('sp_GetPaySlip',           'P') IS NOT NULL DROP PROCEDURE sp_GetPaySlip;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payroll' AND xtype='U')
BEGIN
    CREATE TABLE Payroll (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId      INT            NOT NULL,
        Month           INT            NOT NULL,
        Year            INT            NOT NULL,
        BasicSalary     DECIMAL(10,2)  NOT NULL,
        HouseAllowance  DECIMAL(10,2)  NOT NULL DEFAULT 0,
        TransportAllow  DECIMAL(10,2)  NOT NULL DEFAULT 0,
        OvertimePay     DECIMAL(10,2)  NOT NULL DEFAULT 0,
        TaxDeduction    DECIMAL(10,2)  NOT NULL DEFAULT 0,
        OtherDeductions DECIMAL(10,2)  NOT NULL DEFAULT 0,
        NetSalary       DECIMAL(10,2)  NOT NULL,
        Status          NVARCHAR(20)   NOT NULL DEFAULT 'Draft',  -- Draft | Paid
        PaidOn          DATETIME       NULL,
        GeneratedOn     DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Payroll_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
        CONSTRAINT UQ_Payroll_EmpMonth UNIQUE (EmployeeId, Month, Year)
    );
END
GO

-- Get all payroll records
CREATE PROCEDURE sp_GetAllPayroll
    @Month INT = NULL,
    @Year  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.Id, p.EmployeeId, e.Name AS EmployeeName,
        d.Name AS DepartmentName,
        p.Month, p.Year,
        p.BasicSalary, p.HouseAllowance, p.TransportAllow, p.OvertimePay,
        p.TaxDeduction, p.OtherDeductions, p.NetSalary,
        p.Status, p.PaidOn, p.GeneratedOn
    FROM Payroll p
    INNER JOIN Employees   e ON p.EmployeeId   = e.Id
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE (@Month IS NULL OR p.Month = @Month)
      AND (@Year  IS NULL OR p.Year  = @Year)
    ORDER BY p.Year DESC, p.Month DESC, e.Name;
END
GO

-- Get payroll for a specific employee
CREATE PROCEDURE sp_GetPayrollByEmployee
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.Id, p.EmployeeId, e.Name AS EmployeeName,
        p.Month, p.Year,
        p.BasicSalary, p.HouseAllowance, p.TransportAllow, p.OvertimePay,
        p.TaxDeduction, p.OtherDeductions, p.NetSalary,
        p.Status, p.PaidOn, p.GeneratedOn
    FROM Payroll p
    INNER JOIN Employees e ON p.EmployeeId = e.Id
    WHERE p.EmployeeId = @EmployeeId
    ORDER BY p.Year DESC, p.Month DESC;
END
GO

-- Generate payroll for an employee for a given month
CREATE PROCEDURE sp_GeneratePayroll
    @EmployeeId     INT,
    @Month          INT,
    @Year           INT,
    @HouseAllowance DECIMAL(10,2) = 0,
    @TransportAllow DECIMAL(10,2) = 0,
    @OvertimePay    DECIMAL(10,2) = 0,
    @TaxDeduction   DECIMAL(10,2) = 0,
    @OtherDeductions DECIMAL(10,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @BasicSalary DECIMAL(10,2);
    SELECT @BasicSalary = Salary FROM Employees WHERE Id = @EmployeeId;

    DECLARE @NetSalary DECIMAL(10,2) =
        @BasicSalary + @HouseAllowance + @TransportAllow + @OvertimePay
        - @TaxDeduction - @OtherDeductions;

    IF EXISTS (SELECT 1 FROM Payroll WHERE EmployeeId = @EmployeeId AND Month = @Month AND Year = @Year)
    BEGIN
        UPDATE Payroll
        SET BasicSalary = @BasicSalary, HouseAllowance = @HouseAllowance,
            TransportAllow = @TransportAllow, OvertimePay = @OvertimePay,
            TaxDeduction = @TaxDeduction, OtherDeductions = @OtherDeductions,
            NetSalary = @NetSalary, GeneratedOn = GETDATE()
        WHERE EmployeeId = @EmployeeId AND Month = @Month AND Year = @Year;
    END
    ELSE
    BEGIN
        INSERT INTO Payroll
            (EmployeeId, Month, Year, BasicSalary, HouseAllowance, TransportAllow,
             OvertimePay, TaxDeduction, OtherDeductions, NetSalary)
        VALUES
            (@EmployeeId, @Month, @Year, @BasicSalary, @HouseAllowance, @TransportAllow,
             @OvertimePay, @TaxDeduction, @OtherDeductions, @NetSalary);
    END
END
GO

-- Mark payroll as Paid
CREATE PROCEDURE sp_UpdatePayrollStatus
    @Id     INT,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Payroll
    SET Status = @Status,
        PaidOn = CASE WHEN @Status = 'Paid' THEN GETDATE() ELSE NULL END
    WHERE Id = @Id;
END
GO

-- Get single pay slip
CREATE PROCEDURE sp_GetPaySlip
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.Id, p.EmployeeId, e.Name AS EmployeeName,
        e.Email, d.Name AS DepartmentName,
        p.Month, p.Year,
        p.BasicSalary, p.HouseAllowance, p.TransportAllow, p.OvertimePay,
        p.TaxDeduction, p.OtherDeductions, p.NetSalary,
        p.Status, p.PaidOn, p.GeneratedOn
    FROM Payroll p
    INNER JOIN Employees   e ON p.EmployeeId   = e.Id
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE p.Id = @Id;
END
GO
