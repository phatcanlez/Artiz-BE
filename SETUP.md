# Hướng dẫn Setup Backend

## Bước 1: Cài đặt Dependencies

```bash
cd BE/ArtizBackend
dotnet restore
```

## Bước 2: Cấu hình Database

1. Mở file `ArtizBackend/appsettings.json`
2. Kiểm tra connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ArtizDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```
3. Nếu bạn dùng SQL Server khác, thay đổi connection string tương ứng

## Bước 3: Chạy Backend

```bash
cd ArtizBackend
dotnet run
```

Backend sẽ chạy tại:
- HTTP: `http://localhost:5046`
- HTTPS: `https://localhost:7144`
- Swagger UI: `http://localhost:5046/swagger`

## Bước 4: Seed Dữ liệu mẫu (Tùy chọn)

Sau khi backend đã chạy, bạn có thể seed dữ liệu mẫu bằng cách:

1. Mở Swagger UI: `http://localhost:5046/swagger`
2. Tìm endpoint `POST /api/seed/seed-data`
3. Click "Try it out" và "Execute"
4. Hoặc sử dụng curl:
   ```bash
   curl -X POST http://localhost:5046/api/seed/seed-data
   ```

## Bước 5: Test API

### Đăng ký User mới:
```bash
curl -X POST http://localhost:5046/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "password123",
    "phone": "0123456789"
  }'
```

### Đăng nhập:
```bash
curl -X POST http://localhost:5046/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

### Lấy danh sách sản phẩm:
```bash
curl -X GET http://localhost:5046/api/products
```

## Troubleshooting

### Lỗi kết nối Database
- Đảm bảo SQL Server hoặc LocalDB đang chạy
- Kiểm tra connection string trong appsettings.json
- Thử tạo database thủ công nếu cần

### Lỗi Port đã được sử dụng
- Thay đổi port trong `launchSettings.json`
- Hoặc kill process đang sử dụng port đó

### Lỗi CORS
- Kiểm tra CORS policy trong `Program.cs`
- Đảm bảo frontend URL được thêm vào allowed origins

