
/* =========================================================
   CAR POC - Database Setup
   Database: CARPOC
   SQL Server
   ========================================================= */

USE master;
GO

IF DB_ID('CARPOC') IS NULL
BEGIN
    CREATE DATABASE CARPOC;
END;
GO

USE CARPOC;
GO

/* =========================================================
   1. Departments
   ========================================================= */

IF OBJECT_ID('dbo.EmployeeProjects', 'U') IS NOT NULL
    DROP TABLE dbo.EmployeeProjects;
GO

IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL
    DROP TABLE dbo.Projects;
GO

IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL
    DROP TABLE dbo.Employees;
GO

IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL
    DROP TABLE dbo.Departments;
GO

CREATE TABLE dbo.Departments
(
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* =========================================================
   2. Employees
   ========================================================= */

CREATE TABLE dbo.Employees
(
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Salary DECIMAL(18,2) NOT NULL,
    JoiningDate DATE NOT NULL,
    DepartmentId INT NOT NULL,
    ManagerId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Employees_Departments
        FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(DepartmentId),

    CONSTRAINT FK_Employees_Manager
        FOREIGN KEY (ManagerId)
        REFERENCES dbo.Employees(EmployeeId),

    CONSTRAINT CK_Employees_Salary
        CHECK (Salary >= 0)
);
GO

/* =========================================================
   3. Projects
   ========================================================= */

CREATE TABLE dbo.Projects
(
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    Budget DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,

    CONSTRAINT CK_Projects_Budget
        CHECK (Budget >= 0)
);
GO

/* =========================================================
   4. EmployeeProjects
   Many-to-many relationship
   ========================================================= */

CREATE TABLE dbo.EmployeeProjects
(
    EmployeeProjectId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    ProjectId INT NOT NULL,
    AssignedDate DATE NOT NULL,
    Role NVARCHAR(100) NULL,

    CONSTRAINT FK_EmployeeProjects_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES dbo.Employees(EmployeeId),

    CONSTRAINT FK_EmployeeProjects_Projects
        FOREIGN KEY (ProjectId)
        REFERENCES dbo.Projects(ProjectId),

    CONSTRAINT UQ_EmployeeProjects
        UNIQUE (EmployeeId, ProjectId)
);
GO

/* =========================================================
   5. Indexes
   ========================================================= */

CREATE INDEX IX_Employees_DepartmentId
ON dbo.Employees(DepartmentId);

CREATE INDEX IX_Employees_Salary
ON dbo.Employees(Salary DESC);

CREATE INDEX IX_Employees_JoiningDate
ON dbo.Employees(JoiningDate);

CREATE INDEX IX_Projects_Status
ON dbo.Projects(Status);

CREATE INDEX IX_EmployeeProjects_ProjectId
ON dbo.EmployeeProjects(ProjectId);
GO

/* =========================================================
   6. Insert Departments
   ========================================================= */

INSERT INTO dbo.Departments
    (DepartmentName, Location)
VALUES
    ('IT', 'Noida'),
    ('HR', 'Delhi'),
    ('Finance', 'Gurugram'),
    ('Marketing', 'Mumbai'),
    ('Sales', 'Bengaluru');
GO

/* =========================================================
   7. Insert Employees
   ========================================================= */

INSERT INTO dbo.Employees
(
    FirstName,
    LastName,
    Email,
    Salary,
    JoiningDate,
    DepartmentId,
    ManagerId,
    IsActive
)
VALUES
('Amit', 'Sharma', 'amit@car.com', 95000, '2021-01-15', 1, NULL, 1),
('Rahul', 'Verma', 'rahul@car.com', 85000, '2022-03-10', 1, 1, 1),
('Priya', 'Singh', 'priya@car.com', 90000, '2020-06-20', 1, 1, 1),
('Neha', 'Gupta', 'neha@car.com', 65000, '2023-02-12', 2, NULL, 1),
('Rohit', 'Kumar', 'rohit@car.com', 75000, '2021-08-01', 2, 4, 1),
('Anjali', 'Yadav', 'anjali@car.com', 80000, '2022-11-15', 3, NULL, 1),
('Vikas', 'Mishra', 'vikas@car.com', 70000, '2023-05-10', 3, 6, 1),
('Pooja', 'Mehta', 'pooja@car.com', 60000, '2024-01-05', 4, NULL, 1),
('Sandeep', 'Jain', 'sandeep@car.com', 72000, '2022-07-19', 5, NULL, 1),
('Karan', 'Malhotra', 'karan@car.com', 55000, '2024-03-01', 5, 9, 0);
GO

/* =========================================================
   8. Insert Projects
   ========================================================= */

INSERT INTO dbo.Projects
(
    ProjectName,
    Description,
    StartDate,
    EndDate,
    Budget,
    Status
)
VALUES
('CAR Platform', 'Context augmented retrieval POC', '2025-01-01', NULL, 500000, 'Active'),
('HR Management', 'Employee management system', '2024-01-01', '2025-12-31', 250000, 'Completed'),
('Finance Portal', 'Financial reporting application', '2025-03-01', NULL, 350000, 'Active'),
('Marketing Analytics', 'Marketing data analytics platform', '2025-05-01', NULL, 200000, 'Active');
GO

/* =========================================================
   9. Assign Employees to Projects
   ========================================================= */

INSERT INTO dbo.EmployeeProjects
(
    EmployeeId,
    ProjectId,
    AssignedDate,
    Role
)
VALUES
(1, 1, '2025-01-01', 'Project Lead'),
(2, 1, '2025-01-05', 'Developer'),
(3, 1, '2025-01-10', 'Business Analyst'),
(4, 2, '2024-01-01', 'HR Lead'),
(5, 2, '2024-02-01', 'HR Executive'),
(6, 3, '2025-03-01', 'Finance Lead'),
(7, 3, '2025-03-10', 'Finance Analyst'),
(8, 4, '2025-05-01', 'Marketing Lead'),
(9, 1, '2025-02-01', 'QA Engineer'),
(2, 3, '2025-04-01', 'Developer');
GO

/* =========================================================
   10. Verify Database
   ========================================================= */

SELECT
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

SELECT
    d.DepartmentName,
    COUNT(e.EmployeeId) AS TotalEmployees,
    AVG(e.Salary) AS AverageSalary
FROM dbo.Departments d
LEFT JOIN dbo.Employees e
    ON d.DepartmentId = e.DepartmentId
GROUP BY d.DepartmentName
ORDER BY d.DepartmentName;
GO
