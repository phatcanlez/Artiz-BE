using BO.DTOs;

namespace BLL.Services;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(int id);
    Task<IEnumerable<ReviewDto>> GetByProductIdAsync(int productId);
    Task<ReviewDto> CreateAsync(ReviewCreateDto dto, int? userId = null);
    Task<ReviewDto?> UpdateAsync(int id, int rating, string comment);
    Task<bool> DeleteAsync(int id);
    Task<bool> IncrementHelpfulVotesAsync(int id);
}
