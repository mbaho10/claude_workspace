using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Domain.Entities;

public class ActionPlanItem
{
    public int Id { get; set; }
    public int ActionPlanId { get; set; }
    public int DepartmentId { get; set; }
    public string InitiativeName { get; set; } = string.Empty;
    public string? ResponsiblePerson { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? BudgetAllocated { get; set; }
    public decimal? BudgetSpent { get; set; }
    public ActionPlanStatus Status { get; set; } = ActionPlanStatus.NotStarted;
    public string? Notes { get; set; }
    public int SortOrder { get; set; } = 0;
    public int LastModifiedBy { get; set; }
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ActionPlan ActionPlan { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User LastModifiedByUser { get; set; } = null!;
}
