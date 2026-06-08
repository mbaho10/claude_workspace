using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Application.Services;

public class PermissionService(AppDbContext db) : IPermissionService
{
    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        return user?.Role == UserRole.Admin;
    }

    public async Task<List<int>> GetEditableDepartmentIdsAsync(int userId)
    {
        var user = await db.Users
            .Include(u => u.DepartmentPermissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return [];
        if (user.Role == UserRole.Admin)
            return await db.Departments.Where(d => d.IsActive).Select(d => d.Id).ToListAsync();

        return user.DepartmentPermissions
            .Where(p => p.PermissionLevel == PermissionLevel.Edit)
            .Select(p => p.DepartmentId)
            .ToList();
    }

    public async Task<List<int>> GetViewableDepartmentIdsAsync(int userId)
    {
        var user = await db.Users
            .Include(u => u.DepartmentPermissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return [];
        if (user.Role == UserRole.Admin)
            return await db.Departments.Where(d => d.IsActive).Select(d => d.Id).ToListAsync();

        var permissionedDepts = user.DepartmentPermissions.Select(p => p.DepartmentId).ToList();
        if (user.DepartmentId.HasValue && !permissionedDepts.Contains(user.DepartmentId.Value))
            permissionedDepts.Add(user.DepartmentId.Value);

        return permissionedDepts;
    }

    public async Task<bool> CanEditDepartmentAsync(int userId, int departmentId)
    {
        var editableIds = await GetEditableDepartmentIdsAsync(userId);
        return editableIds.Contains(departmentId);
    }

    public async Task<bool> CanViewDepartmentAsync(int userId, int departmentId)
    {
        var viewableIds = await GetViewableDepartmentIdsAsync(userId);
        return viewableIds.Contains(departmentId);
    }
}
