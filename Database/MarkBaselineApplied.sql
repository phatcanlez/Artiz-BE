-- =============================================
-- Script SQL - Đánh dấu migration Baseline đã được apply
-- Chạy script này SAU KHI đã chạy AddIsAdminColumn.sql
-- =============================================

USE ArtizDb;
GO

-- Kiểm tra và thêm cột IsAdmin nếu chưa tồn tại (backup check)
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

-- Đánh dấu migration Baseline đã được apply (nếu cần)
-- Lưu ý: Migration Baseline sẽ cố tạo lại các bảng đã có, nên không thể apply được
-- Chỉ cần chạy AddIsAdminColumn.sql là đủ
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    IF NOT EXISTS (
        SELECT * FROM __EFMigrationsHistory 
        WHERE MigrationId = '20260109095529_Baseline'
    )
    BEGIN
        -- Không insert vì migration Baseline sẽ fail khi apply
        PRINT 'Lưu ý: Migration Baseline không thể apply vì các bảng đã tồn tại.';
        PRINT 'Chỉ cần chạy AddIsAdminColumn.sql là đủ.';
    END
END
GO

PRINT 'Hoàn tất!';
GO

