namespace BO.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;
    
    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

