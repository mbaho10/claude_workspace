namespace BudgetPlanner.Application.DTOs.Budget;

public record BudgetTemplateDto(
    int Id,
    string Name,
    int Year,
    string PeriodType,
    int PeriodCount,
    bool IsLocked,
    DateTime CreatedAt
);

public record CreateBudgetTemplateRequest(
    string Name,
    int Year,
    string PeriodType
);

public record BudgetGridDataResponse(
    BudgetTemplateDto Template,
    List<BudgetCategoryDto> Categories,
    List<DepartmentDto> Departments,
    List<string> PeriodLabels,
    List<BudgetCellDto> Cells,
    List<int> EditableDepartmentIds
);

public record BudgetCategoryDto(
    int Id,
    string Name,
    int? ParentId,
    int SortOrder,
    bool IsCalculated,
    string? CategoryCode
);

public record DepartmentDto(int Id, string Name, string Code);

public record BudgetCellDto(
    int CategoryId,
    int DepartmentId,
    byte PeriodIndex,
    decimal Amount,
    string? Note
);

public record SaveCellRequest(
    int CategoryId,
    int DepartmentId,
    byte PeriodIndex,
    decimal Amount,
    string? Note
);

public record SaveCellsRequest(List<SaveCellRequest> Cells);

public record CreateCategoryRequest(
    string Name,
    int? ParentId,
    int SortOrder,
    bool IsCalculated,
    string? CategoryCode
);

public record UpdateCategoryRequest(
    string Name,
    int? ParentId,
    int SortOrder,
    bool IsCalculated,
    string? CategoryCode
);
