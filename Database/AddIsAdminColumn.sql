-- =============================================
-- Script SQL - Thêm cột IsAdmin vào bảng Users
-- Chạy script này nếu database đã tồn tại nhưng thiếu cột IsAdmin
-- =============================================

USE ArtizDb;
GO

-- Kiểm tra và thêm cột IsAdmin nếu chưa tồn tại
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'Users') 
    AND name = 'IsAdmin'
)
BEGIN
    ALTER TABLE Users
    ADD IsAdmin BIT NOT NULL DEFAULT 0;
    
    PRINT 'Đã thêm cột IsAdmin vào bảng Users thành công!';
END
ELSE
BEGIN
    PRINT 'Cột IsAdmin đã tồn tại trong bảng Users.';
END
GO

-- Cập nhật admin user (nếu cần)
-- UPDATE Users SET IsAdmin = 1 WHERE Email = 'admin@artiz.com';
-- GO

PRINT 'Hoàn tất!';
GO

