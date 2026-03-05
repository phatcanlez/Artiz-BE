using BO.DTOs;
using BO.Models;
using DAL.Repositories;

namespace BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _productRepository.SearchAsync(searchTerm);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateUpdateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Size = dto.Size,
            Material = dto.Material,
            ProductPolicy = dto.ProductPolicy,
            ProductPreservation = dto.ProductPreservation,
            DeliveryTax = dto.DeliveryTax,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.ThumbnailUrls != null)
        {
            for (var i = 0; i < dto.ThumbnailUrls.Count; i++)
                product.ProductImages.Add(new ProductImage { Url = dto.ThumbnailUrls[i], DisplayOrder = i });
        }
        if (dto.Model3DUrls != null)
        {
            for (var i = 0; i < dto.Model3DUrls.Count; i++)
                product.Product3DFiles.Add(new Product3DFile { Url = dto.Model3DUrls[i], FileName = $"model{i}", DisplayOrder = i });
        }

        var created = await _productRepository.CreateAsync(product);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, ProductCreateUpdateDto dto)
    {
        var product = new Product
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Size = dto.Size,
            Material = dto.Material,
            ProductPolicy = dto.ProductPolicy,
            ProductPreservation = dto.ProductPreservation,
            DeliveryTax = dto.DeliveryTax,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
        };

        if (dto.ThumbnailUrls != null)
        {
            for (var i = 0; i < dto.ThumbnailUrls.Count; i++)
                product.ProductImages.Add(new ProductImage { ProductId = id, Url = dto.ThumbnailUrls[i], DisplayOrder = i });
        }
        if (dto.Model3DUrls != null)
        {
            for (var i = 0; i < dto.Model3DUrls.Count; i++)
                product.Product3DFiles.Add(new Product3DFile { ProductId = id, Url = dto.Model3DUrls[i], FileName = $"model{i}", DisplayOrder = i });
        }

        var updated = await _productRepository.UpdateAsync(product);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    private ProductDto MapToDto(Product product)
    {
        var reviews = product.Reviews?.ToList() ?? new List<Review>();
        var averageRating = reviews.Any() 
            ? reviews.Average(r => r.Rating) 
            : 0.0;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            ThumbnailUrls = product.ProductImages?.Select(pi => pi.Url).ToList() ?? new List<string>(),
            Model3DUrls = product.Product3DFiles?.Select(p3 => p3.Url).ToList() ?? new List<string>(),
            Size = product.Size,
            Material = product.Material,
            ProductPolicy = product.ProductPolicy,
            ProductPreservation = product.ProductPreservation,
            DeliveryTax = product.DeliveryTax,
            Stock = product.Stock,
            AverageRating = averageRating,
            ReviewCount = reviews.Count
        };
    }
}

