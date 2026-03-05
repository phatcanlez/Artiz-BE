using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/health")]
public class DbHealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DbHealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok(new
            {
                connected = canConnect,
                provider = _context.Database.ProviderName
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                connected = false,
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
}

