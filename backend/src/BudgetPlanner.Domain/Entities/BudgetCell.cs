namespace BudgetPlanner.Domain.Entities;

public class BudgetCell
{
    public long Id { get; set; }
    public int TemplateId { get; set; }
    public int CategoryId { get; set; }
    public int DepartmentId { get; set; }
    public byte PeriodIndex { get; set; }
    public decimal Amount { get; set; } = 0;
    public string? Note { get; set; }
    public int LastModifiedBy { get; set; }
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    public BudgetTemplate Template { get; set; } = null!;
    public BudgetCategory Category { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User LastModifiedByUser { get; set; } = null!;
}
