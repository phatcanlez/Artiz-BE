using BO.Models;

namespace DAL.Repositories;

public interface IBlogRepository
{
    Task<IEnumerable<BlogPost>> GetAllAsync();
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<BlogPost> CreateAsync(BlogPost post);
    Task<BlogPost?> UpdateAsync(BlogPost post);
    Task<bool> DeleteAsync(int id);
}


