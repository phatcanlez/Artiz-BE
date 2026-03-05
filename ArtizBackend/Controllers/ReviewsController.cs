using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;
using System.Security.Claims;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>Get all reviews for a product</summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetByProductId(int productId)
    {
        try
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy đánh giá" });
        }
    }

    /// <summary>Get a review by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        try
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null)
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy đánh giá" });
        }
    }

    /// <summary>Create a new review (guest or authenticated user)</summary>
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] ReviewCreateDto dto)
    {
        try
        {
            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var uid))
                userId = uid;

            var created = await _reviewService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo đánh giá" });
        }
    }

    /// <summary>Update a review</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ReviewDto>> Update(int id, [FromBody] ReviewUpdateRequest request)
    {
        try
        {
            var updated = await _reviewService.UpdateAsync(id, request.Rating, request.Comment);
            if (updated == null)
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật đánh giá" });
        }
    }

    /// <summary>Delete a review</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _reviewService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa đánh giá" });
        }
    }

    /// <summary>Mark a review as helpful</summary>
    [HttpPost("{id}/helpful")]
    public async Task<IActionResult> IncrementHelpful(int id)
    {
        try
        {
            var ok = await _reviewService.IncrementHelpfulVotesAsync(id);
            if (!ok)
                return NotFound(new { message = "Không tìm thấy đánh giá" });
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing helpful for review {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi" });
        }
    }

    public record ReviewUpdateRequest(int Rating, string Comment);
}
