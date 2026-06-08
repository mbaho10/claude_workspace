using System.Security.Claims;
using BudgetPlanner.Application.DTOs.Budget;
using BudgetPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Api.Controllers;

[ApiController]
[Route("api/budget")]
[Authorize]
public class BudgetController(IBudgetService budgetService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates([FromQuery] int? year)
        => Ok(await budgetService.GetTemplatesAsync(year));

    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateBudgetTemplateRequest request)
        => Ok(await budgetService.CreateTemplateAsync(request, CurrentUserId));

    [HttpGet("templates/{templateId}")]
    public async Task<IActionResult> GetTemplate(int templateId)
        => Ok(await budgetService.GetTemplateAsync(templateId));

    [HttpPut("templates/{templateId}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LockTemplate(int templateId, [FromQuery] bool locked = true)
    {
        await budgetService.LockTemplateAsync(templateId, locked);
        return NoContent();
    }

    [HttpGet("templates/{templateId}/data")]
    public async Task<IActionResult> GetGridData(int templateId, [FromQuery] int? departmentId)
        => Ok(await budgetService.GetGridDataAsync(templateId, CurrentUserId, departmentId));

    [HttpPut("templates/{templateId}/cells")]
    public async Task<IActionResult> SaveCells(int templateId, [FromBody] SaveCellsRequest request)
    {
        await budgetService.SaveCellsAsync(templateId, request, CurrentUserId);
        return NoContent();
    }

    [HttpPost("templates/{templateId}/categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(int templateId, [FromBody] CreateCategoryRequest request)
        => Ok(await budgetService.CreateCategoryAsync(templateId, request));

    [HttpPut("templates/{templateId}/categories/{categoryId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int templateId, int categoryId, [FromBody] UpdateCategoryRequest request)
        => Ok(await budgetService.UpdateCategoryAsync(templateId, categoryId, request));

    [HttpDelete("templates/{templateId}/categories/{categoryId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int templateId, int categoryId)
    {
        await budgetService.DeleteCategoryAsync(templateId, categoryId);
        return NoContent();
    }

    [HttpGet("templates/{templateId}/export")]
    public async Task<IActionResult> Export(int templateId)
    {
        var bytes = await budgetService.ExportToExcelAsync(templateId, CurrentUserId);
        var template = await budgetService.GetTemplateAsync(templateId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Budget_{template.Name}_{template.Year}.xlsx");
    }
}
