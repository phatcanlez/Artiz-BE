namespace BO.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int HelpfulVotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCreateDto
{
    public int ProductId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

