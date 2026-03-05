using BO.Models;

namespace DAL.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
    Task<Review> CreateAsync(Review review);
    Task<Review?> UpdateAsync(Review review);
    Task<bool> DeleteAsync(int id);
    Task<bool> IncrementHelpfulVotesAsync(int id);
}
