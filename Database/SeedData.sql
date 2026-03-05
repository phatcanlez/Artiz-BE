-- =============================================
-- Script Seed dữ liệu mẫu
-- =============================================

USE ArtizDb;
GO

-- Xóa dữ liệu cũ (nếu có)
DELETE FROM [dbo].[OrderItems];
DELETE FROM [dbo].[Orders];
DELETE FROM [dbo].[Reviews];
DELETE FROM [dbo].[Products];
DELETE FROM [dbo].[Users];
GO

-- Reset Identity
DBCC CHECKIDENT ('[dbo].[Users]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Products]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Reviews]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Orders]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[OrderItems]', RESEED, 0);
GO

-- =============================================
-- Seed Users (Password: password123)
-- Password hash được tạo bằng BCrypt: $2a$11$...
-- =============================================
INSERT INTO [dbo].[Users] ([Name], [Email], [PasswordHash], [Phone], [CreatedAt], [IsActive], [IsAdmin])
VALUES 
    ('Admin User', 'admin@artiz.com', '$2a$11$KIXvKqJqJqJqJqJqJqJqJ.qJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq', '0123456789', GETUTCDATE(), 1, 1),
    ('Nguyễn Văn A', 'nguyenvana@example.com', '$2a$11$KIXvKqJqJqJqJqJqJqJqJ.qJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq', '0987654321', GETUTCDATE(), 1, 0),
    ('Trần Thị B', 'tranthib@example.com', '$2a$11$KIXvKqJqJqJqJqJqJqJqJ.qJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq', '0912345678', GETUTCDATE(), 1, 0);
GO

-- =============================================
-- Seed Products
-- =============================================
INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [Size], [Material], [ProductPolicy], [ProductPreservation], [DeliveryTax], [Stock], [IsActive], [CreatedAt])
VALUES 
    ('Men Armor Black Silver', 
     'Premium armor case với thiết kế hiện đại, bảo vệ tối đa cho điện thoại của bạn. Chất liệu cao cấp, chống sốc và chống trầy xước hiệu quả.',
     3850000,
     'https://images.unsplash.com/photo-1622434641406-a158123450f9?w=400&q=80',
     'Universal',
     'Premium Leather & Metal',
     'Bảo hành 12 tháng, đổi trả trong 30 ngày',
     'Tránh tiếp xúc với nước và nhiệt độ cao',
     'Miễn phí vận chuyển toàn quốc',
     50, 1, GETUTCDATE()),

    ('Men Armor Classic',
     'Thiết kế cổ điển với chất liệu cao cấp, phù hợp cho mọi lứa tuổi. Bảo vệ điện thoại một cách tinh tế và sang trọng.',
     3200000,
     'https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=400&q=80',
     'Universal',
     'Genuine Leather',
     'Bảo hành 12 tháng',
     'Lau sạch bằng vải mềm',
     'Miễn phí vận chuyển',
     30, 1, GETUTCDATE()),

    ('Men Armor Pro',
     'Phiên bản chuyên nghiệp với khả năng chống sốc vượt trội. Thiết kế công thái học, phù hợp cho người dùng thường xuyên di chuyển.',
     4500000,
     'https://images.unsplash.com/photo-1587836374828-4dbafa94cf0e?w=400&q=80',
     'Universal',
     'Carbon Fiber & Metal',
     'Bảo hành 24 tháng',
     'Bảo quản nơi khô ráo',
     'Miễn phí vận chuyển',
     25, 1, GETUTCDATE()),

    ('Men Armor Sport',
     'Thiết kế thể thao năng động, chống nước và bụi bẩn. Phù hợp cho các hoạt động ngoài trời và thể thao.',
     2800000,
     'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&q=80',
     'Universal',
     'Silicone & TPU',
     'Bảo hành 12 tháng',
     'Có thể rửa bằng nước',
     'Miễn phí vận chuyển',
     40, 1, GETUTCDATE()),

    ('Men Armor Luxury',
     'Phiên bản cao cấp với da thật và kim loại quý. Thiết kế sang trọng, phù hợp cho doanh nhân và người thành đạt.',
     5500000,
     'https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400&q=80',
     'Universal',
     'Premium Leather & Gold Accent',
     'Bảo hành 36 tháng',
     'Bảo quản cẩn thận, tránh xước',
     'Miễn phí vận chuyển',
     15, 1, GETUTCDATE()),

    ('Men Armor Black',
     'Thiết kế đơn giản, tinh tế với màu đen cổ điển. Phù hợp cho mọi phong cách và lứa tuổi.',
     2500000,
     'https://images.unsplash.com/photo-1533139502658-0198f920d8e8?w=400&q=80',
     'Universal',
     'Premium Leather',
     'Bảo hành 12 tháng',
     'Lau sạch bằng vải mềm',
     'Miễn phí vận chuyển',
     60, 1, GETUTCDATE()),

    ('Men Armor Silver',
     'Phiên bản bạc sang trọng với viền kim loại. Thiết kế hiện đại, phù hợp cho giới trẻ năng động.',
     3500000,
     'https://images.unsplash.com/photo-1509048191080-d2984bad6ae5?w=400&q=80',
     'Universal',
     'Metal & Leather',
     'Bảo hành 12 tháng',
     'Tránh va chạm mạnh',
     'Miễn phí vận chuyển',
     35, 1, GETUTCDATE()),

    ('Men Armor Carbon',
     'Chất liệu carbon fiber cao cấp, nhẹ và bền. Thiết kế thể thao, phù hợp cho người yêu thích công nghệ.',
     4200000,
     'https://images.unsplash.com/photo-1434056886845-dbd7bc7db535?w=400&q=80',
     'Universal',
     'Carbon Fiber',
     'Bảo hành 18 tháng',
     'Bảo quản nơi khô ráo',
     'Miễn phí vận chuyển',
     20, 1, GETUTCDATE());
GO

-- =============================================
-- Seed Reviews
-- =============================================
INSERT INTO [dbo].[Reviews] ([ProductId], [UserId], [Rating], [Comment], [CreatedAt])
VALUES 
    (1, 2, 5, 'Sản phẩm rất tốt, chất lượng cao. Tôi rất hài lòng!', DATEADD(day, -10, GETUTCDATE())),
    (1, 3, 4, 'Tốt nhưng giá hơi cao một chút. Nhìn chung là ổn.', DATEADD(day, -5, GETUTCDATE())),
    (2, 2, 5, 'Thiết kế đẹp, chất liệu tốt. Đáng giá tiền!', DATEADD(day, -8, GETUTCDATE())),
    (3, 3, 5, 'Rất chắc chắn, bảo vệ tốt. Recommend!', DATEADD(day, -3, GETUTCDATE())),
    (4, 2, 4, 'Phù hợp cho thể thao, chống nước tốt.', DATEADD(day, -7, GETUTCDATE()));
GO

PRINT 'Dữ liệu mẫu đã được seed thành công!';
GO

