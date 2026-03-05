using Microsoft.EntityFrameworkCore;
using BO.Models;

namespace DAL.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly ApplicationDbContext _context;

    public BlogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BlogPost>> GetAllAsync()
    {
        return await _context.BlogPosts
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        return await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        return await _context.BlogPosts.FirstOrDefaultAsync(b => b.Slug == slug);
    }

    public async Task<BlogPost> CreateAsync(BlogPost post)
    {
        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<BlogPost?> UpdateAsync(BlogPost post)
    {
        var existing = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == post.Id);
        if (existing == null)
        {
            return null;
        }

        existing.Title = post.Title;
        existing.Slug = post.Slug;
        existing.Summary = post.Summary;
        existing.Content = post.Content;
        existing.ThumbnailUrl = post.ThumbnailUrl;
        existing.IsPublished = post.IsPublished;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
        if (existing == null)
        {
            return false;
        }

        _context.BlogPosts.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}


