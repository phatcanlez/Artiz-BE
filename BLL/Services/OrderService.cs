using BO.DTOs;
using BO.Models;
using DAL.Repositories;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailEventPublisher _emailEventPublisher;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IEmailEventPublisher emailEventPublisher)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _emailEventPublisher = emailEventPublisher;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("Giỏ hàng trống. Vui lòng thêm ít nhất một sản phẩm trước khi đặt hàng.", nameof(request.Items));

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new ArgumentException("Địa chỉ giao hàng không được để trống.", nameof(request.ShippingAddress));

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("Số điện thoại không được để trống.", nameof(request.Phone));

        decimal total = 0;
        var orderItems = new List<OrderItem>();
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Không tìm thấy sản phẩm có ID: {item.ProductId}. Vui lòng kiểm tra lại giỏ hàng.");
            if (!product.IsActive)
                throw new InvalidOperationException($"Sản phẩm \"{product.Name}\" (ID: {item.ProductId}) hiện không còn bán.");
            if (product.Stock <= 0)
                throw new InvalidOperationException($"Sản phẩm \"{product.Name}\" (ID: {item.ProductId}) đã hết hàng.");
            var qty = Math.Max(1, item.Quantity);
            if (qty > product.Stock)
                throw new InvalidOperationException($"Số lượng đặt của \"{product.Name}\" (ID: {item.ProductId}) vượt quá tồn kho. Tồn kho hiện có: {product.Stock}.");

            var price = item.Price > 0 ? item.Price : product.Price;
            total += price * qty;
            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = qty,
                Price = price
            });
        }

        var shippingCost = 50000m;
        total += shippingCost;

        var addressLine = request.ShippingAddress;
        if (!string.IsNullOrWhiteSpace(request.City))
            addressLine += ", " + request.City;
        if (!string.IsNullOrWhiteSpace(request.PostalCode))
            addressLine += ", " + request.PostalCode;

        var isCod = string.Equals(request.PaymentMethod, "cod", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(request.PaymentMethod, "cash", StringComparison.OrdinalIgnoreCase);
        var initialStatus = isCod ? "Chờ xác nhận" : "Chờ thanh toán";

        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            Status = initialStatus,
            ShippingAddress = addressLine.Trim().TrimStart(',').Trim(),
            Phone = request.Phone
        };

        var created = await _orderRepository.CreateAsync(order, orderItems);
        var orderDto = MapToDto(created);

        // Gửi email xác nhận đơn hàng
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && _emailService.IsConfigured)
            {
                var subject = $"Xác nhận đơn hàng {orderDto.OrderInvoiceNumber}";
                var bodyPlain = $"Xin chào {user.Name},\n\nĐơn hàng {orderDto.OrderInvoiceNumber} của bạn đã được tạo thành công với tổng tiền {total} VND.";
                var bodyHtml = BuildOrderCreatedEmailHtml(user.Name, orderDto);
                await _emailService.SendAsync(user.Email, user.Name, subject, bodyPlain, bodyHtml);
            }

            await _emailEventPublisher.PublishOrderCreatedAsync(userId, orderDto);
        }
        catch
        {
            // Không chặn tạo đơn nếu email lỗi
        }

        return new CreateOrderResponse(created.Id, created.OrderInvoiceNumber, created.TotalAmount);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            return null;
        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    private static OrderDto MapToDto(Order order)
    {
        var items = order.OrderItems.Select(oi => new OrderItemDto(
            oi.ProductId,
            oi.Product?.Name ?? "",
            oi.Quantity,
            oi.Price,
            oi.Product?.ImageUrl ?? string.Empty
        )).ToList();
        return new OrderDto(
            order.Id,
            order.OrderInvoiceNumber,
            order.TotalAmount,
            order.Status,
            order.ShippingAddress,
            order.Phone,
            order.CreatedAt,
            items
        );
    }

    private static string BuildOrderCreatedEmailHtml(string name, OrderDto order)
    {
        var itemsRows = string.Join("", order.Items.Select(i =>
            $@"<tr>
  <td style=""padding:8px 12px;border-bottom:1px solid #27272a;color:#e5e7eb;"">{System.Net.WebUtility.HtmlEncode(i.ProductName)}</td>
  <td style=""padding:8px 12px;border-bottom:1px solid #27272a;color:#e5e7eb;text-align:center;"">{i.Quantity}</td>
  <td style=""padding:8px 12px;border-bottom:1px solid #27272a;color:#e5e7eb;text-align:right;"">{i.Price:N0} VND</td>
</tr>"
        ));

        return $@"
<html>
  <body style=""margin:0;padding:0;background-color:#000000;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#000000;padding:24px 0;"">
      <tr>
        <td align=""center"">
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#111111;border-radius:16px;border:1px solid #333333;padding:32px;color:#F3FAF4;"">
            <tr>
              <td style=""font-size:22px;font-weight:700;padding-bottom:12px;text-align:left;"">
                Đơn hàng {System.Net.WebUtility.HtmlEncode(order.OrderInvoiceNumber)}
              </td>
            </tr>
            <tr>
              <td style=""font-size:14px;line-height:1.6;color:#D1D5DB;padding-bottom:24px;text-align:left;"">
                Xin chào {System.Net.WebUtility.HtmlEncode(name)},<br/><br/>
                Đơn hàng của bạn đã được tạo thành công với tổng tiền <strong style=""color:#44FF00;"">{order.TotalAmount:N0} VND</strong>.
              </td>
            </tr>
            <tr>
              <td>
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;border-radius:12px;overflow:hidden;background-color:#09090b;border:1px solid #27272a;"">
                  <thead>
                    <tr>
                      <th align=""left"" style=""padding:10px 12px;font-size:12px;color:#9ca3af;border-bottom:1px solid #27272a;"">Sản phẩm</th>
                      <th align=""center"" style=""padding:10px 12px;font-size:12px;color:#9ca3af;border-bottom:1px solid #27272a;"">SL</th>
                      <th align=""right"" style=""padding:10px 12px;font-size:12px;color:#9ca3af;border-bottom:1px solid #27272a;"">Giá</th>
                    </tr>
                  </thead>
                  <tbody>
                    {itemsRows}
                  </tbody>
                </table>
              </td>
            </tr>
            <tr>
              <td style=""font-size:12px;color:#6B7280;text-align:left;padding-top:16px;"">
                Bạn có thể xem chi tiết đơn hàng tại trang tài khoản trên website Artiz.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}
