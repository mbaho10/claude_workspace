using BudgetPlanner.Application.DTOs.User;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Application.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await db.Users
            .Include(u => u.Department)
            .OrderBy(u => u.FullName)
            .Select(u => MapUser(u))
            .ToListAsync();
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        var user = await db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("ไม่พบผู้ใช้");
        return MapUser(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("อีเมลนี้มีในระบบแล้ว");

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Enum.Parse<UserRole>(request.Role),
            DepartmentId = request.DepartmentId
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Department).LoadAsync();
        return MapUser(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("ไม่พบผู้ใช้");

        user.FullName = request.FullName;
        user.Role = Enum.Parse<UserRole>(request.Role);
        user.DepartmentId = request.DepartmentId;
        user.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Department).LoadAsync();
        return MapUser(user);
    }

    public async Task<List<UserPermissionDto>> GetUserPermissionsAsync(int userId)
    {
        return await db.UserDepartmentPermissions
            .Include(p => p.Department)
            .Where(p => p.UserId == userId)
            .Select(p => new UserPermissionDto(p.DepartmentId, p.Department.Name, p.PermissionLevel.ToString()))
            .ToListAsync();
    }

    public async Task SetUserPermissionsAsync(int userId, SetPermissionsRequest request)
    {
        var existing = await db.UserDepartmentPermissions.Where(p => p.UserId == userId).ToListAsync();
        db.UserDepartmentPermissions.RemoveRange(existing);

        foreach (var perm in request.Permissions)
        {
            db.UserDepartmentPermissions.Add(new UserDepartmentPermission
            {
                UserId = userId,
                DepartmentId = perm.DepartmentId,
                PermissionLevel = Enum.Parse<PermissionLevel>(perm.PermissionLevel)
            });
        }
        await db.SaveChangesAsync();
    }

    private static UserDto MapUser(User u) =>
        new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.DepartmentId,
            u.Department?.Name, u.IsActive, u.LastLoginAt, u.CreatedAt);
}
