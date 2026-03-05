namespace BO.Models;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? UserId { get; set; } // Nullable for guest reviews
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public int HelpfulVotes { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public User? User { get; set; }
}

