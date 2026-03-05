using BO.Models;

namespace DAL.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, ICollection<OrderItem> items);
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
}
