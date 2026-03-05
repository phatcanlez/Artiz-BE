namespace BO.Models;

public class Product3DFile
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
