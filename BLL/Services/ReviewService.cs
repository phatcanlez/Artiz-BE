using BO.DTOs;
using BO.Models;
using DAL.Repositories;

namespace BLL.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        return review == null ? null : MapToDto(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetByProductIdAsync(int productId)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId);
        return reviews.Select(MapToDto);
    }

    public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto, int? userId = null)
    {
        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            ReviewerName = dto.ReviewerName,
            ReviewerEmail = dto.ReviewerEmail,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _reviewRepository.CreateAsync(review);
        return MapToDto(created);
    }

    public async Task<ReviewDto?> UpdateAsync(int id, int rating, string comment)
    {
        var existing = await _reviewRepository.GetByIdAsync(id);
        if (existing == null)
            return null;

        existing.Rating = rating;
        existing.Comment = comment;
        var updated = await _reviewRepository.UpdateAsync(existing);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _reviewRepository.DeleteAsync(id);
    }

    public async Task<bool> IncrementHelpfulVotesAsync(int id)
    {
        return await _reviewRepository.IncrementHelpfulVotesAsync(id);
    }

    private static ReviewDto MapToDto(Review r)
    {
        return new ReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ReviewerName = r.ReviewerName,
            ReviewerEmail = r.ReviewerEmail,
            Rating = r.Rating,
            Comment = r.Comment,
            HelpfulVotes = r.HelpfulVotes,
            CreatedAt = r.CreatedAt
        };
    }
}
