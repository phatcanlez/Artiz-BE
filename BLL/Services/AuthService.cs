using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BO.DTOs;
using BO.Models;
using DAL.Repositories;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IEmailEventPublisher _emailEventPublisher;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IEmailService emailService,
        IEmailEventPublisher emailEventPublisher)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;
        _emailEventPublisher = emailEventPublisher;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            IsAdmin = user.IsAdmin
        };

        var token = GenerateJwtToken(userDto);

        return new AuthResponse
        {
            Token = token,
            User = userDto
        };
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return null;

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsAdmin = false
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var userDto = new UserDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Phone = createdUser.Phone,
            IsAdmin = createdUser.IsAdmin
        };

        var token = GenerateJwtToken(userDto);

        // Gửi email chào mừng
        try
        {
            if (_emailService.IsConfigured)
            {
                var subject = "Chào mừng đến với Artiz";
                var bodyPlain = $"Xin chào {userDto.Name},\n\nCảm ơn bạn đã tạo tài khoản tại Artiz.";
                var bodyHtml = BuildWelcomeEmailHtml(userDto.Name);
                await _emailService.SendAsync(userDto.Email, userDto.Name, subject, bodyPlain, bodyHtml);
            }
            await _emailEventPublisher.PublishUserRegisteredAsync(userDto);
        }
        catch
        {
            // Không chặn đăng ký nếu email lỗi
        }

        return new AuthResponse
        {
            Token = token,
            User = userDto
        };
    }

    public string GenerateJwtToken(UserDto user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ArtizBackend";
        var audience = jwtSettings["Audience"] ?? "ArtizFrontend";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task RequestPasswordResetAsync(string email, string resetUrlBase)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return;

        var token = GeneratePasswordResetToken(user.Email);
        var resetLink = $"{resetUrlBase}?token={System.Net.WebUtility.UrlEncode(token)}";

        if (_emailService.IsConfigured)
        {
            var subject = "Đặt lại mật khẩu Artiz";
            var bodyPlain = $"Xin chào {user.Name},\n\nNhấn vào liên kết sau để đặt lại mật khẩu: {resetLink}";
            var bodyHtml = BuildResetPasswordEmailHtml(user.Name, resetLink);
            await _emailService.SendAsync(user.Email, user.Name, subject, bodyPlain, bodyHtml);
        }

        await _emailEventPublisher.PublishPasswordResetRequestedAsync(user.Email);
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ArtizBackend";
        var audience = "ArtizPasswordReset";

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        try
        {
            var principal = handler.ValidateToken(token, parameters, out _);
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GeneratePasswordResetToken(string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ArtizBackend";
        var audience = "ArtizPasswordReset";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string BuildResetPasswordEmailHtml(string name, string resetLink)
    {
        return $@"
<html>
  <body style=""margin:0;padding:0;background-color:#000000;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#000000;padding:24px 0;"">
      <tr>
        <td align=""center"">
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#111111;border-radius:16px;border:1px solid #333333;padding:32px;color:#F3FAF4;"">
            <tr>
              <td style=""font-size:22px;font-weight:700;padding-bottom:12px;text-align:left;"">
                Đặt lại mật khẩu
              </td>
            </tr>
            <tr>
              <td style=""font-size:14px;line-height:1.6;color:#D1D5DB;padding-bottom:24px;text-align:left;"">
                Xin chào {System.Net.WebUtility.HtmlEncode(name)},<br/><br/>
                Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản <strong style=""color:#44FF00;"">Artiz</strong>.
                Nhấn vào nút bên dưới để tiếp tục. Liên kết này sẽ hết hạn sau 30 phút.
              </td>
            </tr>
            <tr>
              <td align=""center"" style=""padding-bottom:24px;"">
                <a href=""{System.Net.WebUtility.HtmlEncode(resetLink)}""
                   style=""display:inline-block;padding:12px 32px;border-radius:999px;background-color:#44FF00;color:#102314;font-weight:700;font-size:14px;text-decoration:none;"">
                  ĐẶT LẠI MẬT KHẨU
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:12px;color:#6B7280;text-align:left;"">
                Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }

    private static string BuildWelcomeEmailHtml(string name)
    {
        return $@"
<html>
  <body style=""margin:0;padding:0;background-color:#000000;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#000000;padding:24px 0;"">
      <tr>
        <td align=""center"">
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#111111;border-radius:16px;border:1px solid #333333;padding:32px;color:#F3FAF4;"">
            <tr>
              <td align=""center"" style=""padding-bottom:24px;"">
                <img src=""https://api.builder.io/api/v1/image/assets/TEMP/02de465c6f80619f3b40f7852f664ccbdcc42d26"" alt=""Artiz"" style=""height:64px;border-radius:12px;"" />
              </td>
            </tr>
            <tr>
              <td style=""font-size:24px;font-weight:700;padding-bottom:12px;text-align:left;"">
                Chào mừng, {System.Net.WebUtility.HtmlEncode(name)} 👋
              </td>
            </tr>
            <tr>
              <td style=""font-size:14px;line-height:1.6;color:#D1D5DB;padding-bottom:24px;text-align:left;"">
                Cảm ơn bạn đã tạo tài khoản tại <strong style=""color:#44FF00;"">Artiz</strong>. Từ bây giờ bạn có thể đăng nhập, xem sản phẩm 3D, thêm vào giỏ hàng và theo dõi đơn hàng một cách dễ dàng.
              </td>
            </tr>
            <tr>
              <td align=""center"" style=""padding-bottom:24px;"">
                <a href=""{System.Net.WebUtility.HtmlEncode("http://localhost:5173")}""
                   style=""display:inline-block;padding:12px 32px;border-radius:999px;background-color:#44FF00;color:#102314;font-weight:700;font-size:14px;text-decoration:none;"">
                  BẮT ĐẦU MUA SẮM
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:12px;color:#6B7280;text-align:left;"">
                Nếu bạn không tạo tài khoản này, vui lòng bỏ qua email.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}

