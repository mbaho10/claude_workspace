using BudgetPlanner.Application.DTOs.Budget;

namespace BudgetPlanner.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetTemplateDto>> GetTemplatesAsync(int? year = null);
    Task<BudgetTemplateDto> CreateTemplateAsync(CreateBudgetTemplateRequest request, int userId);
    Task<BudgetTemplateDto> GetTemplateAsync(int templateId);
    Task LockTemplateAsync(int templateId, bool locked);
    Task<BudgetGridDataResponse> GetGridDataAsync(int templateId, int userId, int? departmentId = null);
    Task SaveCellsAsync(int templateId, SaveCellsRequest request, int userId);
    Task<BudgetCategoryDto> CreateCategoryAsync(int templateId, CreateCategoryRequest request);
    Task<BudgetCategoryDto> UpdateCategoryAsync(int templateId, int categoryId, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(int templateId, int categoryId);
    Task<byte[]> ExportToExcelAsync(int templateId, int userId);
}
