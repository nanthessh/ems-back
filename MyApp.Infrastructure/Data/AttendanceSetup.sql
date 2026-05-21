-- ============================================================
-- EMS - Attendance Module
-- ============================================================

IF OBJECT_ID('sp_GetAllAttendance',           'P') IS NOT NULL DROP PROCEDURE sp_GetAllAttendance;
IF OBJECT_ID('sp_GetAttendanceByEmployee',    'P') IS NOT NULL DROP PROCEDURE sp_GetAttendanceByEmployee;
IF OBJECT_ID('sp_GetAttendanceByDate',        'P') IS NOT NULL DROP PROCEDURE sp_GetAttendanceByDate;
IF OBJECT_ID('sp_CheckIn',                    'P') IS NOT NULL DROP PROCEDURE sp_CheckIn;
IF OBJECT_ID('sp_CheckOut',                   'P') IS NOT NULL DROP PROCEDURE sp_CheckOut;
IF OBJECT_ID('sp_GetTodayAttendance',         'P') IS NOT NULL DROP PROCEDURE sp_GetTodayAttendance;
IF OBJECT_ID('sp_GetAttendanceSummary',       'P') IS NOT NULL DROP PROCEDURE sp_GetAttendanceSummary;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Attendance' AND xtype='U')
BEGIN
    CREATE TABLE Attendance (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId  INT          NOT NULL,
        Date        DATE         NOT NULL DEFAULT CAST(GETDATE() AS DATE),
        CheckIn     TIME         NULL,
        CheckOut    TIME         NULL,
        Status      NVARCHAR(20) NOT NULL DEFAULT 'Present',  -- Present | Absent | Late | Half-Day
        Notes       NVARCHAR(255) NULL,
        CONSTRAINT FK_Attendance_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
        CONSTRAINT UQ_Attendance_EmpDate UNIQUE (EmployeeId, Date)
    );
END
GO

-- Get all attendance with employee name
CREATE PROCEDURE sp_GetAllAttendance
    @Month INT = NULL,
    @Year  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.Id, a.EmployeeId, e.Name AS EmployeeName,
        d.Name AS DepartmentName,
        a.Date, a.CheckIn, a.CheckOut, a.Status, a.Notes
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.Id
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE (@Month IS NULL OR MONTH(a.Date) = @Month)
      AND (@Year  IS NULL OR YEAR(a.Date)  = @Year)
    ORDER BY a.Date DESC, e.Name;
END
GO

-- Get attendance for a specific employee
CREATE PROCEDURE sp_GetAttendanceByEmployee
    @EmployeeId INT,
    @Month      INT = NULL,
    @Year       INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.Id, a.EmployeeId, e.Name AS EmployeeName,
        a.Date, a.CheckIn, a.CheckOut, a.Status, a.Notes
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.Id
    WHERE a.EmployeeId = @EmployeeId
      AND (@Month IS NULL OR MONTH(a.Date) = @Month)
      AND (@Year  IS NULL OR YEAR(a.Date)  = @Year)
    ORDER BY a.Date DESC;
END
GO

-- Get today's attendance
CREATE PROCEDURE sp_GetTodayAttendance
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.Id, a.EmployeeId, e.Name AS EmployeeName,
        d.Name AS DepartmentName,
        a.Date, a.CheckIn, a.CheckOut, a.Status, a.Notes
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.Id
    INNER JOIN Departments d ON e.DepartmentId = d.Id
    WHERE a.Date = CAST(GETDATE() AS DATE)
    ORDER BY a.CheckIn;
END
GO

-- Check In
CREATE PROCEDURE sp_CheckIn
    @EmployeeId INT,
    @Notes      NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @Now   TIME = CAST(GETDATE() AS TIME);
    DECLARE @Status NVARCHAR(20) = 'Present';

    -- Mark Late if check-in after 09:30
    IF @Now > '09:30:00' SET @Status = 'Late';

    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND Date = @Today)
    BEGIN
        UPDATE Attendance SET CheckIn = @Now, Status = @Status, Notes = @Notes
        WHERE EmployeeId = @EmployeeId AND Date = @Today;
    END
    ELSE
    BEGIN
        INSERT INTO Attendance (EmployeeId, Date, CheckIn, Status, Notes)
        VALUES (@EmployeeId, @Today, @Now, @Status, @Notes);
    END
END
GO

-- Check Out
CREATE PROCEDURE sp_CheckOut
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @Now   TIME = CAST(GETDATE() AS TIME);

    UPDATE Attendance SET CheckOut = @Now
    WHERE EmployeeId = @EmployeeId AND Date = @Today;
END
GO

-- Monthly summary per employee
CREATE PROCEDURE sp_GetAttendanceSummary
    @EmployeeId INT,
    @Month      INT,
    @Year       INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*)                                        AS TotalDays,
        SUM(CASE WHEN Status = 'Present'  THEN 1 ELSE 0 END) AS PresentDays,
        SUM(CASE WHEN Status = 'Absent'   THEN 1 ELSE 0 END) AS AbsentDays,
        SUM(CASE WHEN Status = 'Late'     THEN 1 ELSE 0 END) AS LateDays,
        SUM(CASE WHEN Status = 'Half-Day' THEN 1 ELSE 0 END) AS HalfDays
    FROM Attendance
    WHERE EmployeeId = @EmployeeId
      AND MONTH(Date) = @Month
      AND YEAR(Date)  = @Year;
END
GO
