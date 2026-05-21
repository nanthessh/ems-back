-- ============================================================
-- EMS - Announcements Module
-- ============================================================

IF OBJECT_ID('sp_GetAllAnnouncements',  'P') IS NOT NULL DROP PROCEDURE sp_GetAllAnnouncements;
IF OBJECT_ID('sp_GetAnnouncementById',  'P') IS NOT NULL DROP PROCEDURE sp_GetAnnouncementById;
IF OBJECT_ID('sp_CreateAnnouncement',   'P') IS NOT NULL DROP PROCEDURE sp_CreateAnnouncement;
IF OBJECT_ID('sp_UpdateAnnouncement',   'P') IS NOT NULL DROP PROCEDURE sp_UpdateAnnouncement;
IF OBJECT_ID('sp_DeleteAnnouncement',   'P') IS NOT NULL DROP PROCEDURE sp_DeleteAnnouncement;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Announcements' AND xtype='U')
BEGIN
    CREATE TABLE Announcements (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Title       NVARCHAR(200)  NOT NULL,
        Content     NVARCHAR(MAX)  NOT NULL,
        Priority    NVARCHAR(20)   NOT NULL DEFAULT 'Normal',  -- Low | Normal | High | Urgent
        PostedBy    NVARCHAR(100)  NOT NULL,
        PostedOn    DATETIME       NOT NULL DEFAULT GETDATE(),
        ExpiresOn   DATE           NULL
    );
END
GO

CREATE PROCEDURE sp_GetAllAnnouncements
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Content, Priority, PostedBy, PostedOn, ExpiresOn
    FROM Announcements
    WHERE ExpiresOn IS NULL OR ExpiresOn >= CAST(GETDATE() AS DATE)
    ORDER BY PostedOn DESC;
END
GO

CREATE PROCEDURE sp_GetAnnouncementById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Content, Priority, PostedBy, PostedOn, ExpiresOn
    FROM Announcements WHERE Id = @Id;
END
GO

CREATE PROCEDURE sp_CreateAnnouncement
    @Title     NVARCHAR(200),
    @Content   NVARCHAR(MAX),
    @Priority  NVARCHAR(20),
    @PostedBy  NVARCHAR(100),
    @ExpiresOn DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Announcements (Title, Content, Priority, PostedBy, ExpiresOn)
    VALUES (@Title, @Content, @Priority, @PostedBy, @ExpiresOn);
END
GO

CREATE PROCEDURE sp_UpdateAnnouncement
    @Id        INT,
    @Title     NVARCHAR(200),
    @Content   NVARCHAR(MAX),
    @Priority  NVARCHAR(20),
    @ExpiresOn DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Announcements
    SET Title = @Title, Content = @Content, Priority = @Priority, ExpiresOn = @ExpiresOn
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE sp_DeleteAnnouncement
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Announcements WHERE Id = @Id;
END
GO
