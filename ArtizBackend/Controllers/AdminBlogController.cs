using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/admin/blog")]
[Authorize(Roles = "Admin")]
public class AdminBlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ILogger<AdminBlogController> _logger;

    public AdminBlogController(IBlogService blogService, ILogger<AdminBlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetPosts()
    {
        try
        {
            var posts = await _blogService.GetAllAsync();
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách bài viết" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPostDto>> GetPost(int id)
    {
        try
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy bài viết" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BlogPostDto>> CreatePost([FromBody] BlogPostCreateUpdateDto request)
    {
        try
        {
            var created = await _blogService.CreateAsync(request);
            return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo bài viết" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BlogPostDto>> UpdatePost(int id, [FromBody] BlogPostCreateUpdateDto request)
    {
        try
        {
            var updated = await _blogService.UpdateAsync(id, request);
            if (updated == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật bài viết" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            var deleted = await _blogService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa bài viết" });
        }
    }
}


