using Microsoft.EntityFrameworkCore;
using BO.Models;
using DAL;

namespace DAL.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order, ICollection<OrderItem> items)
    {
        order.OrderInvoiceNumber = "DH" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100, 999).ToString();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in items)
        {
            item.OrderId = order.Id;
            _context.OrderItems.Add(item);
        }
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
