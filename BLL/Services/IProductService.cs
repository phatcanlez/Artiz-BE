using BO.DTOs;

namespace BLL.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<ProductDto> CreateProductAsync(ProductCreateUpdateDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, ProductCreateUpdateDto dto);
    Task<bool> DeleteProductAsync(int id);
}

