using BO.DTOs;
using BO.Models;
using DAL.Repositories;

namespace BLL.Services;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _blogRepository;

    public BlogService(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<IEnumerable<BlogPostDto>> GetAllAsync()
    {
        var posts = await _blogRepository.GetAllAsync();
        return posts.Select(MapToDto);
    }

    public async Task<BlogPostDto?> GetByIdAsync(int id)
    {
        var post = await _blogRepository.GetByIdAsync(id);
        return post == null ? null : MapToDto(post);
    }

    public async Task<BlogPostDto> CreateAsync(BlogPostCreateUpdateDto dto)
    {
        var post = new BlogPost
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            Content = dto.Content,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsPublished = dto.IsPublished
        };

        var created = await _blogRepository.CreateAsync(post);
        return MapToDto(created);
    }

    public async Task<BlogPostDto?> UpdateAsync(int id, BlogPostCreateUpdateDto dto)
    {
        var post = new BlogPost
        {
            Id = id,
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            Content = dto.Content,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsPublished = dto.IsPublished
        };

        var updated = await _blogRepository.UpdateAsync(post);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _blogRepository.DeleteAsync(id);
    }

    private static BlogPostDto MapToDto(BlogPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        Content = post.Content,
        ThumbnailUrl = post.ThumbnailUrl,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
        IsPublished = post.IsPublished
    };
}


