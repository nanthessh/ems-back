-- Run DepartmentSetup.sql first, then DatabaseSetup.sql, then AuthSetup.sql

-- Departments
CREATE TABLE Departments (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);

-- Employees
CREATE TABLE Employees (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100)  NOT NULL,
    Email        NVARCHAR(100)  NOT NULL UNIQUE,
    DepartmentId INT            NOT NULL,
    Salary       DECIMAL(10,2)  NOT NULL,
    JoinedDate   DATE           NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

-- Users
CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256)  NOT NULL,
    Role         NVARCHAR(50)   NOT NULL DEFAULT 'User',
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
);
