using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng nhập" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email và mật khẩu là bắt buộc" });
            }

            var response = await _authService.RegisterAsync(request);
            if (response == null)
            {
                return Conflict(new { message = "Email đã được sử dụng" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng ký" });
        }
    }

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email là bắt buộc" });

            var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetUrlBase = $"{frontendBase.TrimEnd('/')}/reset-password";

            await _authService.RequestPasswordResetAsync(request.Email, resetUrlBase);

            return Ok(new { message = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu quên mật khẩu" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Token và mật khẩu mới là bắt buộc" });

            var ok = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!ok)
                return BadRequest(new { message = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn" });

            return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reset password");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đặt lại mật khẩu" });
        }
    }
}

