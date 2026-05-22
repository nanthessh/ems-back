-- ============================================================
-- EMS - PostgreSQL Schema for Supabase
-- Run this entire script in Supabase SQL Editor
-- ============================================================

-- Departments
CREATE TABLE IF NOT EXISTS departments (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL
);

-- Employees
CREATE TABLE IF NOT EXISTS employees (
    id            SERIAL PRIMARY KEY,
    name          VARCHAR(100)   NOT NULL,
    email         VARCHAR(100)   NOT NULL UNIQUE,
    department_id INT            NOT NULL,
    salary        DECIMAL(10,2)  NOT NULL,
    joined_date   DATE           NOT NULL DEFAULT CURRENT_DATE,
    CONSTRAINT fk_employees_departments FOREIGN KEY (department_id) REFERENCES departments(id)
);

-- Users
CREATE TABLE IF NOT EXISTS users (
    id            SERIAL PRIMARY KEY,
    username      VARCHAR(100)  NOT NULL UNIQUE,
    password_hash VARCHAR(256)  NOT NULL,
    role          VARCHAR(50)   NOT NULL DEFAULT 'User',
    created_at    TIMESTAMP     NOT NULL DEFAULT NOW()
);

-- Attendance
CREATE TABLE IF NOT EXISTS attendance (
    id          SERIAL PRIMARY KEY,
    employee_id INT          NOT NULL,
    date        DATE         NOT NULL DEFAULT CURRENT_DATE,
    check_in    TIME         NULL,
    check_out   TIME         NULL,
    status      VARCHAR(20)  NOT NULL DEFAULT 'Present',
    notes       VARCHAR(255) NULL,
    CONSTRAINT fk_attendance_employee FOREIGN KEY (employee_id) REFERENCES employees(id),
    CONSTRAINT uq_attendance_emp_date UNIQUE (employee_id, date)
);

-- Leave Types
CREATE TABLE IF NOT EXISTS leave_types (
    id       SERIAL PRIMARY KEY,
    name     VARCHAR(50) NOT NULL UNIQUE,
    max_days INT         NOT NULL DEFAULT 10
);

INSERT INTO leave_types (name, max_days) VALUES
    ('Sick Leave',      10),
    ('Casual Leave',     7),
    ('Annual Leave',    15),
    ('Maternity Leave', 90),
    ('Unpaid Leave',    30)
ON CONFLICT (name) DO NOTHING;

-- Leave Requests
CREATE TABLE IF NOT EXISTS leave_requests (
    id            SERIAL PRIMARY KEY,
    employee_id   INT           NOT NULL,
    leave_type_id INT           NOT NULL,
    start_date    DATE          NOT NULL,
    end_date      DATE          NOT NULL,
    total_days    INT           NOT NULL,
    reason        VARCHAR(500)  NOT NULL,
    status        VARCHAR(20)   NOT NULL DEFAULT 'Pending',
    admin_note    VARCHAR(255)  NULL,
    applied_on    TIMESTAMP     NOT NULL DEFAULT NOW(),
    reviewed_on   TIMESTAMP     NULL,
    CONSTRAINT fk_leave_employee  FOREIGN KEY (employee_id)   REFERENCES employees(id),
    CONSTRAINT fk_leave_type      FOREIGN KEY (leave_type_id) REFERENCES leave_types(id)
);

-- Payroll
CREATE TABLE IF NOT EXISTS payroll (
    id               SERIAL PRIMARY KEY,
    employee_id      INT            NOT NULL,
    month            INT            NOT NULL,
    year             INT            NOT NULL,
    basic_salary     DECIMAL(10,2)  NOT NULL,
    house_allowance  DECIMAL(10,2)  NOT NULL DEFAULT 0,
    transport_allow  DECIMAL(10,2)  NOT NULL DEFAULT 0,
    overtime_pay     DECIMAL(10,2)  NOT NULL DEFAULT 0,
    tax_deduction    DECIMAL(10,2)  NOT NULL DEFAULT 0,
    other_deductions DECIMAL(10,2)  NOT NULL DEFAULT 0,
    net_salary       DECIMAL(10,2)  NOT NULL,
    status           VARCHAR(20)    NOT NULL DEFAULT 'Draft',
    paid_on          TIMESTAMP      NULL,
    generated_on     TIMESTAMP      NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_payroll_employee FOREIGN KEY (employee_id) REFERENCES employees(id),
    CONSTRAINT uq_payroll_emp_month UNIQUE (employee_id, month, year)
);

-- Announcements
CREATE TABLE IF NOT EXISTS announcements (
    id         SERIAL PRIMARY KEY,
    title      VARCHAR(200) NOT NULL,
    content    TEXT         NOT NULL,
    priority   VARCHAR(20)  NOT NULL DEFAULT 'Normal',
    posted_by  VARCHAR(100) NOT NULL,
    posted_on  TIMESTAMP    NOT NULL DEFAULT NOW(),
    expires_on DATE         NULL
);
