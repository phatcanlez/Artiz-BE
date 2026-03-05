using BO.DTOs;
using DAL.Repositories;

namespace BLL.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;

    public AdminUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Phone = u.Phone,
            IsAdmin = u.IsAdmin
        });
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        await _userRepository.SetActiveAsync(id, isActive);
    }
}


