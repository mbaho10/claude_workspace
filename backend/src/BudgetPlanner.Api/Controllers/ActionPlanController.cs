using System.Security.Claims;
using BudgetPlanner.Application.DTOs.ActionPlan;
using BudgetPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Api.Controllers;

[ApiController]
[Route("api/action-plans")]
[Authorize]
public class ActionPlanController(IActionPlanService actionPlanService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? templateId)
        => Ok(await actionPlanService.GetActionPlansAsync(templateId));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateActionPlanRequest request)
        => Ok(await actionPlanService.CreateActionPlanAsync(request, CurrentUserId));

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(int id)
        => Ok(await actionPlanService.GetItemsAsync(id, CurrentUserId));

    [HttpPost("{id}/items")]
    public async Task<IActionResult> CreateItem(int id, [FromBody] CreateActionPlanItemRequest request)
        => Ok(await actionPlanService.CreateItemAsync(id, request, CurrentUserId));

    [HttpPut("{id}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] UpdateActionPlanItemRequest request)
        => Ok(await actionPlanService.UpdateItemAsync(id, itemId, request, CurrentUserId));

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(int id, int itemId)
    {
        await actionPlanService.DeleteItemAsync(id, itemId, CurrentUserId);
        return NoContent();
    }

    [HttpPut("{id}/items/reorder")]
    public async Task<IActionResult> ReorderItems(int id, [FromBody] ReorderItemsRequest request)
    {
        await actionPlanService.ReorderItemsAsync(id, request, CurrentUserId);
        return NoContent();
    }
}
