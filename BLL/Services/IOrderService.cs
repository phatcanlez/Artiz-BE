using BO.DTOs;

namespace BLL.Services;

public interface IOrderService
{
    Task<CreateOrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId);
    Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId);
}
