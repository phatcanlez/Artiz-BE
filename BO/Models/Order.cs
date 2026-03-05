namespace BO.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    /// <summary>Mã đơn hàng (VD: DH20260304123456001) dùng cho SePay, hiển thị.</summary>
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
    public string? ShippingAddress { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

