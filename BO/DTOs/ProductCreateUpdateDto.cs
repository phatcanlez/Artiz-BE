namespace BO.DTOs;

public class ProductCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string>? ThumbnailUrls { get; set; }
    public List<string>? Model3DUrls { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? ProductPolicy { get; set; }
    public string? ProductPreservation { get; set; }
    public string? DeliveryTax { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}


