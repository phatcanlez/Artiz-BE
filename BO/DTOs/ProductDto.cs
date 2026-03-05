namespace BO.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> ThumbnailUrls { get; set; } = new();
    public List<string> Model3DUrls { get; set; } = new();
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? ProductPolicy { get; set; }
    public string? ProductPreservation { get; set; }
    public string? DeliveryTax { get; set; }
    public int Stock { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

