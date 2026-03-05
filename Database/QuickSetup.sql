-- =============================================
-- Script SQL nhanh - Tạo Database và Tables
-- Chạy script này trong SQL Server Management Studio
-- =============================================

-- Tạo Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ArtizDb')
    CREATE DATABASE ArtizDb;
GO

USE ArtizDb;
GO

-- Tạo bảng Users
IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Phone NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsAdmin BIT NOT NULL DEFAULT 0
    );
END
GO

-- Tạo bảng Products
IF OBJECT_ID('Products', 'U') IS NULL
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        ImageUrl NVARCHAR(MAX) NOT NULL,
        Size NVARCHAR(255) NULL,
        Material NVARCHAR(255) NULL,
        ProductPolicy NVARCHAR(MAX) NULL,
        ProductPreservation NVARCHAR(MAX) NULL,
        DeliveryTax NVARCHAR(MAX) NULL,
        Stock INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END
GO

-- Tạo bảng Reviews
IF OBJECT_ID('Reviews', 'U') IS NULL
BEGIN
    CREATE TABLE Reviews (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        UserId INT NOT NULL,
        Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- Tạo bảng Orders
IF OBJECT_ID('Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        ShippingAddress NVARCHAR(MAX) NULL,
        Phone NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- Tạo bảng OrderItems
IF OBJECT_ID('OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE OrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
    );
END
GO

PRINT 'Database và các bảng đã được tạo thành công!';
PRINT 'Bạn có thể seed dữ liệu bằng cách:';
PRINT '1. Sử dụng API: POST /api/seed/seed-data';
PRINT '2. Hoặc chạy script SeedData.sql';
GO

