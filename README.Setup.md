# Artiz Backend - Setup Guide

## 1. Kết nối Supabase (PostgreSQL)

Backend dùng **Supabase** làm database (PostgreSQL).

### Lấy connection string từ Supabase

1. Đăng nhập [Supabase Dashboard](https://supabase.com/dashboard)
2. Chọn project → **Settings** → **Database**
3. Trong **Connection string** chọn **URI** hoặc **Connection pooling** (port 5432 hoặc 6543)
4. Copy connection string, ví dụ:
   - Direct: `Host=db.xxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true`
   - Hoặc URI: `postgresql://postgres:YOUR_PASSWORD@db.xxxx.supabase.co:5432/postgres` (cần đổi thành format Npgsql như trên)

### Cấu hình appsettings

Trong `appsettings.json` hoặc `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=db.xxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
}
```

Thay `db.xxxx.supabase.co`, `YOUR_PASSWORD` bằng thông tin project của bạn.

### Chạy migrations (lần đầu)

```bash
cd BE/ArtizBackend
dotnet ef migrations add InitialSupabase --project DAL --startup-project ArtizBackend
dotnet ef database update --project DAL --startup-project ArtizBackend
```

---

## 2. Cloudflare R2 – Lưu ảnh và file 3D

### Tạo R2 Bucket

1. Đăng nhập [Cloudflare Dashboard](https://dash.cloudflare.com)
2. Vào **R2 Object Storage** → **Create bucket** → đặt tên `artiz-storage`
3. Bật **Public access** (nếu muốn file public) hoặc dùng **Custom domain**

### Tạo API Token

1. Vào **R2** → **Manage R2 API Tokens** → **Create API token**
2. Chọn quyền **Object Read & Write**
3. Lưu **Access Key ID** và **Secret Access Key**
4. **Account ID**: xem trong R2 dashboard (URL hoặc Overview)

### Cấu hình appsettings

```json
"CloudflareR2": {
  "AccountId": "YOUR_ACCOUNT_ID",
  "AccessKeyId": "YOUR_ACCESS_KEY_ID",
  "SecretAccessKey": "YOUR_SECRET_ACCESS_KEY",
  "BucketName": "artiz-storage",
  "PublicBaseUrl": "https://your-public-domain.com"
}
```

- **PublicBaseUrl**: custom domain (vd: `https://cdn.yoursite.com`) hoặc r2.dev public URL (vd: `https://pub-xxxx.r2.dev` từ bucket settings)

### Chế độ Development (không dùng R2)

Nếu không điền R2 (hoặc để `AccountId` = `YOUR_ACCOUNT_ID`), backend tự động dùng **LocalStorage** – file lưu vào `./uploads` và serve qua `/uploads`.

---

## 3. Cấu hình gửi mail (SMTP)

Backend dùng **SMTP** để gửi email (quên mật khẩu, xác nhận đơn hàng, v.v.).

### Cấu hình appsettings

```json
"Email": {
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "EnableSsl": true,
  "UserName": "your-email@example.com",
  "Password": "YOUR_APP_PASSWORD",
  "FromDisplayName": "Artiz",
  "FromAddress": "noreply@artiz.com"
}
```

Ví dụ với Gmail:

- **SmtpHost**: `smtp.gmail.com`
- **SmtpPort**: `587`
- **EnableSsl**: `true`
- **UserName**: địa chỉ Gmail
- **Password**: [App Password](https://myaccount.google.com/apppasswords) (không dùng mật khẩu đăng nhập Gmail)

Ví dụ với SendGrid / Mailgun / SMTP khác: điền đúng host, port và thông tin đăng nhập.

### Sử dụng trong code

Inject `IEmailService` và gọi:

```csharp
await _emailService.SendAsync(
    toEmail: "user@example.com",
    toName: "User",
    subject: "Tiêu đề",
    bodyPlain: "Nội dung văn bản",
    bodyHtml: "<p>Nội dung HTML</p>"
);
```

Kiểm tra đã cấu hình: `if (_emailService.IsConfigured) { ... }`

---

## Migrations (Code First)

```bash
# Tạo migration mới (Supabase/PostgreSQL)
dotnet ef migrations add MigrationName --project DAL --startup-project ArtizBackend

# Áp dụng migrations
dotnet ef database update --project DAL --startup-project ArtizBackend
```

## API Endpoints (Swagger)

Sau khi chạy `dotnet run`, mở Swagger: `https://localhost:5046/swagger`

### CRUD APIs

| Entity | Endpoints |
|--------|-----------|
| **Products** | GET/POST /api/products, GET/PUT/DELETE /api/products/{id} |
| **Admin Products** | GET/POST /api/admin/products, PUT/DELETE /api/admin/products/{id} |
| **Reviews** | GET /api/reviews/product/{productId}, POST /api/reviews, GET/PUT/DELETE /api/reviews/{id} |
| **Orders (Admin)** | GET /api/admin/orders, PUT /api/admin/orders/{id}/status |
| **Blog (Admin)** | GET/POST /api/admin/blog, PUT/DELETE /api/admin/blog/{id} |
| **Storage** | POST /api/storage/upload/image, POST /api/storage/upload/3d (Admin) |

### Product thuộc tính (theo giao diện FE)

- Name, Description, Price, ImageUrl (ảnh chính)
- ThumbnailUrls (mảng ảnh phụ)
- Model3DUrls (mảng file .glb/.gltf)
- Size, Material, ProductPolicy, ProductPreservation, DeliveryTax
- Stock, AverageRating, ReviewCount

### Review thuộc tính

- ReviewerName, ReviewerEmail, Rating (1-5), Comment
- HelpfulVotes, CreatedAt
