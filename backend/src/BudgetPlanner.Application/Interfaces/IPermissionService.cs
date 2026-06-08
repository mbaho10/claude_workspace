namespace BudgetPlanner.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> CanEditDepartmentAsync(int userId, int departmentId);
    Task<bool> CanViewDepartmentAsync(int userId, int departmentId);
    Task<List<int>> GetEditableDepartmentIdsAsync(int userId);
    Task<List<int>> GetViewableDepartmentIdsAsync(int userId);
    Task<bool> IsAdminAsync(int userId);
}
