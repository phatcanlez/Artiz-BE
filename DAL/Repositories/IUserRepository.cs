using BO.Models;

namespace DAL.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task SetActiveAsync(int id, bool isActive);
}

