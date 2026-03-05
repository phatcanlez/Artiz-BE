using Microsoft.AspNetCore.Mvc;
using DAL;
using BO.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private const string AdminEmail = "admin@artiz.com";
    private const string AdminPassword = "1";

    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(ApplicationDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Tạo tài khoản admin (email: admin@artiz.com, mật khẩu: 1) nếu chưa tồn tại.</summary>
    [HttpPost("seed-admin")]
    public async Task<IActionResult> SeedAdmin()
    {
        try
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == AdminEmail);
            if (existing != null)
            {
                return Ok(new
                {
                    message = "Tài khoản admin đã tồn tại",
                    email = AdminEmail,
                    hint = "Đăng nhập với mật khẩu đã cấu hình (hoặc mật khẩu: 1 nếu vừa seed)"
                });
            }

            var admin = new User
            {
                Name = "Admin",
                Email = AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                IsActive = true,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seed admin user created: {Email}", AdminEmail);
            return Ok(new
            {
                message = "Đã tạo tài khoản admin",
                email = AdminEmail,
                password = AdminPassword
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding admin");
            return StatusCode(500, new { message = "Lỗi khi seed admin", error = ex.Message });
        }
    }

    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            // Đảm bảo có admin trước khi seed products
            if (!await _context.Users.AnyAsync(u => u.Email == AdminEmail))
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = AdminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                    IsActive = true,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(admin);
                await _context.SaveChangesAsync();
            }

            // Check if products already exist
            if (await _context.Products.AnyAsync())
            {
                return BadRequest(new { message = "Database đã có dữ liệu sản phẩm" });
            }

            // Seed Products
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Men Armor Black Silver",
                    Description = "Premium armor case với thiết kế hiện đại, bảo vệ tối đa cho điện thoại của bạn.",
                    Price = 3850000,
                    ImageUrl = "https://images.unsplash.com/photo-1622434641406-a158123450f9?w=400&q=80",
                    Size = "Universal",
                    Material = "Premium Leather & Metal",
                    ProductPolicy = "Bảo hành 12 tháng, đổi trả trong 30 ngày",
                    ProductPreservation = "Tránh tiếp xúc với nước và nhiệt độ cao",
                    DeliveryTax = "Miễn phí vận chuyển toàn quốc",
                    Stock = 50,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Men Armor Classic",
                    Description = "Thiết kế cổ điển với chất liệu cao cấp, phù hợp cho mọi lứa tuổi.",
                    Price = 3200000,
                    ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=400&q=80",
                    Size = "Universal",
                    Material = "Genuine Leather",
                    ProductPolicy = "Bảo hành 12 tháng",
                    ProductPreservation = "Lau sạch bằng vải mềm",
                    DeliveryTax = "Miễn phí vận chuyển",
                    Stock = 30,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Men Armor Pro",
                    Description = "Phiên bản chuyên nghiệp với khả năng chống sốc vượt trội.",
                    Price = 4500000,
                    ImageUrl = "https://images.unsplash.com/photo-1587836374828-4dbafa94cf0e?w=400&q=80",
                    Size = "Universal",
                    Material = "Carbon Fiber & Metal",
                    ProductPolicy = "Bảo hành 24 tháng",
                    ProductPreservation = "Bảo quản nơi khô ráo",
                    DeliveryTax = "Miễn phí vận chuyển",
                    Stock = 25,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Men Armor Sport",
                    Description = "Thiết kế thể thao năng động, chống nước và bụi bẩn.",
                    Price = 2800000,
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&q=80",
                    Size = "Universal",
                    Material = "Silicone & TPU",
                    ProductPolicy = "Bảo hành 12 tháng",
                    ProductPreservation = "Có thể rửa bằng nước",
                    DeliveryTax = "Miễn phí vận chuyển",
                    Stock = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Men Armor Luxury",
                    Description = "Phiên bản cao cấp với da thật và kim loại quý.",
                    Price = 5500000,
                    ImageUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400&q=80",
                    Size = "Universal",
                    Material = "Premium Leather & Gold Accent",
                    ProductPolicy = "Bảo hành 36 tháng",
                    ProductPreservation = "Bảo quản cẩn thận, tránh xước",
                    DeliveryTax = "Miễn phí vận chuyển",
                    Stock = 15,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã seed dữ liệu thành công", count = products.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
            return StatusCode(500, new { message = "Lỗi khi seed dữ liệu", error = ex.Message });
        }
    }
}

