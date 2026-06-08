namespace BudgetPlanner.Application.DTOs.ActionPlan;

public record ActionPlanDto(
    int Id,
    int TemplateId,
    string Name,
    string? Description,
    bool IsLocked,
    DateTime CreatedAt
);

public record CreateActionPlanRequest(int TemplateId, string Name, string? Description);

public record ActionPlanItemDto(
    int Id,
    int ActionPlanId,
    int DepartmentId,
    string DepartmentName,
    string InitiativeName,
    string? ResponsiblePerson,
    string? StartDate,
    string? EndDate,
    decimal? BudgetAllocated,
    decimal? BudgetSpent,
    string Status,
    string? Notes,
    int SortOrder,
    DateTime LastModifiedAt
);

public record CreateActionPlanItemRequest(
    int DepartmentId,
    string InitiativeName,
    string? ResponsiblePerson,
    string? StartDate,
    string? EndDate,
    decimal? BudgetAllocated,
    decimal? BudgetSpent,
    string Status,
    string? Notes
);

public record UpdateActionPlanItemRequest(
    int DepartmentId,
    string InitiativeName,
    string? ResponsiblePerson,
    string? StartDate,
    string? EndDate,
    decimal? BudgetAllocated,
    decimal? BudgetSpent,
    string Status,
    string? Notes,
    int SortOrder
);

public record ReorderItemsRequest(List<int> OrderedIds);
