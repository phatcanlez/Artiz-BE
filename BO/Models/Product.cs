namespace BO.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty; // Main image URL
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? ProductPolicy { get; set; }
    public string? ProductPreservation { get; set; }
    public string? DeliveryTax { get; set; }
    public int Stock { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<Product3DFile> Product3DFiles { get; set; } = new List<Product3DFile>();
}

