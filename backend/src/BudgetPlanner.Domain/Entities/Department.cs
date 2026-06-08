namespace BudgetPlanner.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<UserDepartmentPermission> Permissions { get; set; } = [];
    public ICollection<BudgetCell> BudgetCells { get; set; } = [];
    public ICollection<ActionPlanItem> ActionPlanItems { get; set; } = [];
}
