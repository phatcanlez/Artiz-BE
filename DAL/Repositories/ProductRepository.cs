using Microsoft.EntityFrameworkCore;
using BO.Models;
using DAL;

namespace DAL.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Reviews)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.Product3DFiles.OrderBy(p3 => p3.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Reviews)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.Product3DFiles.OrderBy(p3 => p3.DisplayOrder))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Reviews)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.Product3DFiles.OrderBy(p3 => p3.DisplayOrder))
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _context.Products
            .Include(p => p.Reviews)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.Product3DFiles.OrderBy(p3 => p3.DisplayOrder))
            .Where(p => p.IsActive && 
                (p.Name.Contains(searchTerm) || 
                 p.Description.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        var existing = await _context.Products
            .Include(p => p.ProductImages)
            .Include(p => p.Product3DFiles)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null)
            return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.ImageUrl = product.ImageUrl;
        existing.Size = product.Size;
        existing.Material = product.Material;
        existing.ProductPolicy = product.ProductPolicy;
        existing.ProductPreservation = product.ProductPreservation;
        existing.DeliveryTax = product.DeliveryTax;
        existing.Stock = product.Stock;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.ProductImages.RemoveRange(existing.ProductImages);
        _context.Product3DFiles.RemoveRange(existing.Product3DFiles);
        foreach (var pi in product.ProductImages)
        {
            pi.ProductId = product.Id;
            _context.ProductImages.Add(pi);
        }
        foreach (var p3 in product.Product3DFiles)
        {
            p3.ProductId = product.Id;
            _context.Product3DFiles.Add(p3);
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null)
        {
            return false;
        }

        _context.Products.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}

