namespace BudgetPlanner.Domain.Entities;

public class ActionPlan
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsLocked { get; set; } = false;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BudgetTemplate Template { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ActionPlanItem> Items { get; set; } = [];
}
