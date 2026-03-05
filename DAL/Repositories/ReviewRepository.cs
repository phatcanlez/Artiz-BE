using Microsoft.EntityFrameworkCore;
using BO.Models;
using DAL;

namespace DAL.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<Review?> UpdateAsync(Review review)
    {
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
        if (existing == null)
            return null;

        existing.Rating = review.Rating;
        existing.Comment = review.Comment;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (existing == null)
            return false;

        _context.Reviews.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementHelpfulVotesAsync(int id)
    {
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (existing == null)
            return false;

        existing.HelpfulVotes++;
        await _context.SaveChangesAsync();
        return true;
    }
}
