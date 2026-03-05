using Microsoft.EntityFrameworkCore;
using BO.Models;
using DAL;

namespace DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return;
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

