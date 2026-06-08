using BudgetPlanner.Application.DTOs.ActionPlan;

namespace BudgetPlanner.Application.Interfaces;

public interface IActionPlanService
{
    Task<List<ActionPlanDto>> GetActionPlansAsync(int? templateId = null);
    Task<ActionPlanDto> CreateActionPlanAsync(CreateActionPlanRequest request, int userId);
    Task<List<ActionPlanItemDto>> GetItemsAsync(int actionPlanId, int userId);
    Task<ActionPlanItemDto> CreateItemAsync(int actionPlanId, CreateActionPlanItemRequest request, int userId);
    Task<ActionPlanItemDto> UpdateItemAsync(int actionPlanId, int itemId, UpdateActionPlanItemRequest request, int userId);
    Task DeleteItemAsync(int actionPlanId, int itemId, int userId);
    Task ReorderItemsAsync(int actionPlanId, ReorderItemsRequest request, int userId);
}
