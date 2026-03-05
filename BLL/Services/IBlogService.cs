using BO.DTOs;

namespace BLL.Services;

public interface IBlogService
{
    Task<IEnumerable<BlogPostDto>> GetAllAsync();
    Task<BlogPostDto?> GetByIdAsync(int id);
    Task<BlogPostDto> CreateAsync(BlogPostCreateUpdateDto dto);
    Task<BlogPostDto?> UpdateAsync(int id, BlogPostCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}


