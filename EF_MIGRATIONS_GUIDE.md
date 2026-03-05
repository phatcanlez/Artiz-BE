# Hướng dẫn dùng Entity Framework Core Migrations (Code First)

Repo backend của bạn đã được cấu hình theo **code-first** (models + `ApplicationDbContext`). Phần này sẽ hướng dẫn chi tiết cách tạo và cập nhật migrations trong môi trường Windows hiện tại.

## 1. Chuẩn bị

### 1.1. Cài `dotnet-ef` CLI (nếu chưa có)

Mở `cmd` hoặc PowerShell và chạy:

```bash
dotnet tool install --global dotnet-ef
```

Nếu đã cài rồi nhưng version cũ:

```bash
dotnet tool update --global dotnet-ef
```

Kiểm tra:

```bash
dotnet ef --version
```

### 1.2. Cấu trúc projects

- Startup project (web API): `ArtizBackend/ArtizBackend.csproj`
- DbContext + models: `DAL/DAL.csproj`

`DAL.csproj` đã được thêm:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

## 2. Tạo migration đầu tiên (InitialCreate)

Chạy trong thư mục gốc backend `BE/ArtizBackend`:

```bash
cd BE/ArtizBackend

dotnet ef migrations add InitialCreate ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj ^
  -o DAL\Migrations
```

Giải thích:

- `-p` (hoặc `--project`): project chứa `ApplicationDbContext` → `DAL\DAL.csproj`
- `-s` (hoặc `--startup-project`): project web API dùng để bootstrapping cấu hình DbContext → `ArtizBackend\ArtizBackend.csproj`
- `-o` (hoặc `--output-dir`): thư mục chứa file migration sinh ra → `DAL\Migrations`

Sau khi chạy xong, trong `DAL/Migrations` sẽ có:

- `YYYYMMDDHHMMSS_InitialCreate.cs` – file migration
- `ApplicationDbContextModelSnapshot.cs` – snapshot schema hiện tại

## 3. Áp dụng migration vào database

Vẫn ở `BE/ArtizBackend`:

```bash
dotnet ef database update ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj
```

Lệnh này sẽ:

- Tạo database `ArtizDb` (nếu chưa có) dựa theo connection string trong `ArtizBackend/appsettings.json`.
- Tạo tất cả bảng: `Users`, `Products`, `Reviews`, `Orders`, `OrderItems`, `BlogPosts` với các cột mới như `IsAdmin`, v.v.

> Lưu ý: nếu bạn đang dùng script SQL (`QuickSetup.sql` / `SeedData.sql`), nên dùng **một kiểu** (migration hoặc script) để tránh xung đột schema. Code hiện tại tương thích cho cả hai.

## 4. Khi có thay đổi model (ví dụ: thêm field mới)

Ví dụ bạn thêm trường mới vào model (chẳng hạn `User.IsAdmin`, `BlogPost`... như trong code):

1. Sửa models / `ApplicationDbContext` như bình thường.
2. Tạo migration mới:

```bash
cd BE/ArtizBackend

dotnet ef migrations add AddAdminAndBlogSupport ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj ^
  -o DAL\Migrations
```

3. Áp dụng migration:

```bash
dotnet ef database update ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj
```

## 5. Ví dụ nội dung migration (chỉ tham khảo)

EF sẽ tự sinh file kiểu như sau (nội dung minh họa, KHÔNG cần tự gõ):

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Email = table.Column<string>(maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    IsAdmin = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // ... các bảng Products, Reviews, Orders, OrderItems, BlogPosts ...
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Users");
            // ... drop các bảng khác ...
        }
    }
}
```

Bạn **không cần** tự tạo file như trên – chỉ cần dùng lệnh `dotnet ef migrations add ...`, EF sẽ sinh tự động.

## 6. Một số lỗi thường gặp

### 6.1. Không tìm thấy `dotnet-ef`

> `No executable found matching command "dotnet-ef"`

–> Cài tool:

```bash
dotnet tool install --global dotnet-ef
```

### 6.2. Không tìm thấy DbContext

Kiểm tra:

- `ApplicationDbContext` nằm trong project `DAL`.
- `ArtizBackend` đã đăng ký DbContext trong `Program.cs`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Và nhớ chỉ định đúng `-p DAL\DAL.csproj -s ArtizBackend\ArtizBackend.csproj`.

### 6.3. Muốn xóa migration cuối cùng

Nếu vừa tạo migration sai và chưa chạy `database update`:

```bash
dotnet ef migrations remove -p DAL\DAL.csproj -s ArtizBackend\ArtizBackend.csproj
```

### 6.4. Lỗi "Invalid column name 'IsAdmin'"

Nếu database đã được tạo trước khi thêm property `IsAdmin` vào model `User`, bạn sẽ gặp lỗi:

> `Invalid column name 'IsAdmin'`

**Giải pháp nhanh**: Chạy SQL script để thêm cột:

```sql
-- File: Database/AddIsAdminColumn.sql
USE ArtizDb;
GO

IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'Users') 
    AND name = 'IsAdmin'
)
BEGIN
    ALTER TABLE Users
    ADD IsAdmin BIT NOT NULL DEFAULT 0;
END
GO
```

Hoặc chạy file script có sẵn trong SQL Server Management Studio:

1. Mở file `BE/ArtizBackend/Database/AddIsAdminColumn.sql`
2. Chạy script trong SSMS

**Giải pháp dài hạn**: Tạo migration để đồng bộ schema:

```bash
cd BE/ArtizBackend

dotnet ef migrations add AddIsAdminColumn ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj ^
  -o DAL\Migrations

dotnet ef database update ^
  -p DAL\DAL.csproj ^
  -s ArtizBackend\ArtizBackend.csproj
```

---

Nếu bạn muốn, có thể dùng luôn migrations thay cho các script SQL hiện tại, và mình có thể giúp bạn tạo migration thật (`InitialCreate`) bằng lệnh ở trên để thống nhất về sau chỉ cần `dotnet ef database update` mỗi lần deploy.

