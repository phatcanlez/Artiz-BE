using BO.Models;

namespace DAL.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}

