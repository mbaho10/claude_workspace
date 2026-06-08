namespace BudgetPlanner.Domain.Entities;

public class BudgetCategory
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsCalculated { get; set; } = false;
    public string? CategoryCode { get; set; }
    public bool IsActive { get; set; } = true;

    public BudgetTemplate Template { get; set; } = null!;
    public BudgetCategory? Parent { get; set; }
    public ICollection<BudgetCategory> Children { get; set; } = [];
    public ICollection<BudgetCell> Cells { get; set; } = [];
}
