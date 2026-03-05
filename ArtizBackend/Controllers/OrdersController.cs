using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>Tạo đơn hàng (giỏ hàng + thông tin giao hàng). Trả về OrderId, OrderInvoiceNumber, TotalAmount để FE có thể chuyển sang SePay nếu cần.</summary>
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderRequest? request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Bạn cần đăng nhập để đặt hàng." });

        if (request == null)
            return BadRequest(new { message = "Thiếu dữ liệu đơn hàng. Vui lòng gửi thông tin giao hàng và danh sách sản phẩm." });

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "Giỏ hàng trống. Vui lòng thêm ít nhất một sản phẩm trước khi đặt hàng." });

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            return BadRequest(new { message = "Địa chỉ giao hàng không được để trống." });

        if (string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest(new { message = "Số điện thoại không được để trống." });

        try
        {
            var result = await _orderService.CreateOrderAsync(userId.Value, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            return StatusCode(500, new
            {
                message = "Đã xảy ra lỗi hệ thống khi tạo đơn hàng. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.",
                detail = ex.Message
            });
        }
    }

    /// <summary>Lấy đơn hàng của tôi theo ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(id, userId.Value);
        if (order == null)
            return NotFound(new { message = "Không tìm thấy đơn hàng" });
        return Ok(order);
    }

    /// <summary>Danh sách đơn hàng của tôi.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var orders = await _orderService.GetMyOrdersAsync(userId.Value);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my orders");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách đơn hàng" });
        }
    }
}
