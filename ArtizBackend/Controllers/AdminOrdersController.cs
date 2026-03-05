using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminOrdersController> _logger;

    public AdminOrdersController(ApplicationDbContext context, ILogger<AdminOrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetOrders()
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    UserName = o.User.Name,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.Name,
                        oi.Quantity,
                        oi.Price
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách đơn hàng" });
        }
    }

    public record UpdateOrderStatusRequest(string Status);

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng" });
            }

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng" });
        }
    }
}


