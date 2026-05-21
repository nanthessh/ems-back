-- ============================================================
-- EMS - Leave Management Module
-- ============================================================

IF OBJECT_ID('sp_GetLeaveTypes',           'P') IS NOT NULL DROP PROCEDURE sp_GetLeaveTypes;
IF OBJECT_ID('sp_GetAllLeaveRequests',     'P') IS NOT NULL DROP PROCEDURE sp_GetAllLeaveRequests;
IF OBJECT_ID('sp_GetLeaveByEmployee',      'P') IS NOT NULL DROP PROCEDURE sp_GetLeaveByEmployee;
IF OBJECT_ID('sp_ApplyLeave',              'P') IS NOT NULL DROP PROCEDURE sp_ApplyLeave;
IF OBJECT_ID('sp_UpdateLeaveStatus',       'P') IS NOT NULL DROP PROCEDURE sp_UpdateLeaveStatus;
IF OBJECT_ID('sp_DeleteLeaveRequest',      'P') IS NOT NULL DROP PROCEDURE sp_DeleteLeaveRequest;
IF OBJECT_ID('sp_GetLeaveBalance',         'P') IS NOT NULL DROP PROCEDURE sp_GetLeaveBalance;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LeaveTypes' AND xtype='U')
BEGIN
    CREATE TABLE LeaveTypes (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(50)  NOT NULL UNIQUE,   -- Sick | Casual | Annual | Maternity | Unpaid
        MaxDays     INT           NOT NULL DEFAULT 10
    );
    -- Seed default leave types
    INSERT INTO LeaveTypes (Name, MaxDays) VALUES
        ('Sick Leave',      10),
        ('Casual Leave',     7),
        ('Annual Leave',    15),
        ('Maternity Leave', 90),
        ('Unpaid Leave',    30);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LeaveRequests' AND xtype='U')
BEGIN
    CREATE TABLE LeaveRequests (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId  INT           NOT NULL,
        LeaveTypeId INT           NOT NULL,
        StartDate   DATE          NOT NULL,
        EndDate     DATE          NOT NULL,
        TotalDays   INT           NOT NULL,
        Reason      NVARCHAR(500) NOT NULL,
        Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',  -- Pending | Approved | Rejected
        AdminNote   NVARCHAR(255) NULL,
        AppliedOn   DATETIME      NOT NULL DEFAULT GETDATE(),
        ReviewedOn  DATETIME      NULL,
        CONSTRAINT FK_Leave_Employee  FOREIGN KEY (EmployeeId)  REFERENCES Employees(Id),
        CONSTRAINT FK_Leave_LeaveType FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id)
    );
END
GO

-- Get all leave types
CREATE PROCEDURE sp_GetLeaveTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, MaxDays FROM LeaveTypes ORDER BY Name;
END
GO

-- Get all leave requests (Admin view)
CREATE PROCEDURE sp_GetAllLeaveRequests
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        lr.Id, lr.EmployeeId, e.Name AS EmployeeName,
        d.Name AS DepartmentName,
        lt.Name AS LeaveTypeName,
        lr.StartDate, lr.EndDate, lr.TotalDays,
        lr.Reason, lr.Status, lr.AdminNote,
        lr.AppliedOn, lr.ReviewedOn
    FROM LeaveRequests lr
    INNER JOIN Employees  e  ON lr.EmployeeId  = e.Id
    INNER JOIN Departments d  ON e.DepartmentId = d.Id
    INNER JOIN LeaveTypes  lt ON lr.LeaveTypeId = lt.Id
    WHERE (@Status IS NULL OR lr.Status = @Status)
    ORDER BY lr.AppliedOn DESC;
END
GO

-- Get leave requests for a specific employee
CREATE PROCEDURE sp_GetLeaveByEmployee
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        lr.Id, lr.EmployeeId, e.Name AS EmployeeName,
        lt.Name AS LeaveTypeName, lr.LeaveTypeId,
        lr.StartDate, lr.EndDate, lr.TotalDays,
        lr.Reason, lr.Status, lr.AdminNote,
        lr.AppliedOn, lr.ReviewedOn
    FROM LeaveRequests lr
    INNER JOIN Employees e  ON lr.EmployeeId  = e.Id
    INNER JOIN LeaveTypes lt ON lr.LeaveTypeId = lt.Id
    WHERE lr.EmployeeId = @EmployeeId
    ORDER BY lr.AppliedOn DESC;
END
GO

-- Apply for leave
CREATE PROCEDURE sp_ApplyLeave
    @EmployeeId  INT,
    @LeaveTypeId INT,
    @StartDate   DATE,
    @EndDate     DATE,
    @Reason      NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalDays INT = DATEDIFF(DAY, @StartDate, @EndDate) + 1;
    INSERT INTO LeaveRequests (EmployeeId, LeaveTypeId, StartDate, EndDate, TotalDays, Reason)
    VALUES (@EmployeeId, @LeaveTypeId, @StartDate, @EndDate, @TotalDays, @Reason);
END
GO

-- Approve or Reject leave (Admin)
CREATE PROCEDURE sp_UpdateLeaveStatus
    @Id        INT,
    @Status    NVARCHAR(20),
    @AdminNote NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE LeaveRequests
    SET Status = @Status, AdminNote = @AdminNote, ReviewedOn = GETDATE()
    WHERE Id = @Id;
END
GO

-- Delete leave request (only Pending)
CREATE PROCEDURE sp_DeleteLeaveRequest
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM LeaveRequests WHERE Id = @Id AND Status = 'Pending';
END
GO

-- Leave balance for an employee (current year)
CREATE PROCEDURE sp_GetLeaveBalance
    @EmployeeId INT,
    @Year       INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        lt.Id AS LeaveTypeId,
        lt.Name AS LeaveTypeName,
        lt.MaxDays,
        ISNULL(SUM(CASE WHEN lr.Status = 'Approved' THEN lr.TotalDays ELSE 0 END), 0) AS UsedDays,
        lt.MaxDays - ISNULL(SUM(CASE WHEN lr.Status = 'Approved' THEN lr.TotalDays ELSE 0 END), 0) AS RemainingDays
    FROM LeaveTypes lt
    LEFT JOIN LeaveRequests lr
        ON lt.Id = lr.LeaveTypeId
        AND lr.EmployeeId = @EmployeeId
        AND YEAR(lr.StartDate) = @Year
    GROUP BY lt.Id, lt.Name, lt.MaxDays
    ORDER BY lt.Name;
END
GO
