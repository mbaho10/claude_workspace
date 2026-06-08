using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Domain.Entities;

public class UserDepartmentPermission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DepartmentId { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.ReadOnly;

    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
