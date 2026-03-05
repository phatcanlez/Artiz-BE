using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _userService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(IAdminUserService userService, ILogger<AdminUsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách tài khoản" });
        }
    }

    public record UpdateUserStatusRequest(bool IsActive);

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            await _userService.SetActiveAsync(id, request.IsActive);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật trạng thái tài khoản" });
        }
    }
}


