using BO.DTOs;

namespace BLL.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    string GenerateJwtToken(UserDto user);

    Task RequestPasswordResetAsync(string email, string resetUrlBase);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}

