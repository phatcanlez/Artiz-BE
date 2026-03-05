-- =============================================
-- Script SQL - Tạo Tables cho Database SU25LionDB
-- Chạy script này trong SQL Server Management Studio
-- =============================================

USE SU25LionDB;
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
    PRINT 'Đã tạo bảng Users thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Users đã tồn tại.';
    -- Kiểm tra và thêm cột IsAdmin nếu chưa có
    IF NOT EXISTS (
        SELECT * 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'Users') 
        AND name = 'IsAdmin'
    )
    BEGIN
        ALTER TABLE Users
        ADD IsAdmin BIT NOT NULL DEFAULT 0;
        PRINT 'Đã thêm cột IsAdmin vào bảng Users!';
    END
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
    PRINT 'Đã tạo bảng Products thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Products đã tồn tại.';
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
    PRINT 'Đã tạo bảng Reviews thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Reviews đã tồn tại.';
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
    PRINT 'Đã tạo bảng Orders thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Orders đã tồn tại.';
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
    PRINT 'Đã tạo bảng OrderItems thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng OrderItems đã tồn tại.';
END
GO

-- Tạo bảng BlogPosts
IF OBJECT_ID('BlogPosts', 'U') IS NULL
BEGIN
    CREATE TABLE BlogPosts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL UNIQUE,
        Summary NVARCHAR(MAX) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        ThumbnailUrl NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsPublished BIT NOT NULL DEFAULT 1
    );
    PRINT 'Đã tạo bảng BlogPosts thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng BlogPosts đã tồn tại.';
END
GO

PRINT 'Hoàn tất setup database SU25LionDB!';
PRINT 'Bạn có thể seed dữ liệu bằng cách:';
PRINT '1. Sử dụng API: POST /api/seed/seed-data';
PRINT '2. Hoặc chạy script SeedData.sql (cần sửa database name)';
GO

