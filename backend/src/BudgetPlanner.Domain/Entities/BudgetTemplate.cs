using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Domain.Entities;

public class BudgetTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public PeriodType PeriodType { get; set; } = PeriodType.Monthly;
    public bool IsLocked { get; set; } = false;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Creator { get; set; } = null!;
    public ICollection<BudgetCategory> Categories { get; set; } = [];
    public ICollection<BudgetCell> Cells { get; set; } = [];
    public ICollection<ActionPlan> ActionPlans { get; set; } = [];
}
