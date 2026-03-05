using BO.DTOs;

namespace BLL.Services;

public interface IAdminUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task SetActiveAsync(int id, bool isActive);
}


