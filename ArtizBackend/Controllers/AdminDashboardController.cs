using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(IAdminDashboardService dashboardService, ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy dữ liệu dashboard" });
        }
    }
}


