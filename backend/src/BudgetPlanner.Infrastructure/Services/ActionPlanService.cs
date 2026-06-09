using BudgetPlanner.Application.DTOs.ActionPlan;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Infrastructure.Services;

public class ActionPlanService(AppDbContext db, IPermissionService permissionService) : IActionPlanService
{
    public async Task<List<ActionPlanDto>> GetActionPlansAsync(int? templateId = null)
    {
        var query = db.ActionPlans.AsQueryable();
        if (templateId.HasValue) query = query.Where(a => a.TemplateId == templateId.Value);
        return await query.OrderByDescending(a => a.CreatedAt)
            .Select(a => new ActionPlanDto(a.Id, a.TemplateId, a.Name, a.Description, a.IsLocked, a.CreatedAt))
            .ToListAsync();
    }

    public async Task<ActionPlanDto> CreateActionPlanAsync(CreateActionPlanRequest request, int userId)
    {
        var plan = new ActionPlan
        {
            TemplateId = request.TemplateId,
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId
        };
        db.ActionPlans.Add(plan);
        await db.SaveChangesAsync();
        return new ActionPlanDto(plan.Id, plan.TemplateId, plan.Name, plan.Description, plan.IsLocked, plan.CreatedAt);
    }

    public async Task<List<ActionPlanItemDto>> GetItemsAsync(int actionPlanId, int userId)
    {
        var viewableIds = await permissionService.GetViewableDepartmentIdsAsync(userId);
        var isAdmin = await permissionService.IsAdminAsync(userId);

        var query = db.ActionPlanItems
            .Include(i => i.Department)
            .Where(i => i.ActionPlanId == actionPlanId);

        if (!isAdmin)
            query = query.Where(i => viewableIds.Contains(i.DepartmentId));

        return await query.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .Select(i => MapItem(i))
            .ToListAsync();
    }

    public async Task<ActionPlanItemDto> CreateItemAsync(int actionPlanId, CreateActionPlanItemRequest request, int userId)
    {
        var canEdit = await permissionService.CanEditDepartmentAsync(userId, request.DepartmentId);
        if (!canEdit) throw new UnauthorizedAccessException("ไม่มีสิทธิ์เพิ่มข้อมูลของแผนกนี้");

        var plan = await db.ActionPlans.FindAsync(actionPlanId)
            ?? throw new KeyNotFoundException("ไม่พบ Action Plan");
        if (plan.IsLocked) throw new InvalidOperationException("Action Plan นี้ถูกล็อคแล้ว");

        var maxOrder = await db.ActionPlanItems.Where(i => i.ActionPlanId == actionPlanId).MaxAsync(i => (int?)i.SortOrder) ?? 0;

        var item = new ActionPlanItem
        {
            ActionPlanId = actionPlanId,
            DepartmentId = request.DepartmentId,
            InitiativeName = request.InitiativeName,
            ResponsiblePerson = request.ResponsiblePerson,
            StartDate = ParseDate(request.StartDate),
            EndDate = ParseDate(request.EndDate),
            BudgetAllocated = request.BudgetAllocated,
            BudgetSpent = request.BudgetSpent,
            Status = Enum.Parse<ActionPlanStatus>(request.Status),
            Notes = request.Notes,
            SortOrder = maxOrder + 1,
            LastModifiedBy = userId
        };
        db.ActionPlanItems.Add(item);
        await db.SaveChangesAsync();

        await db.Entry(item).Reference(i => i.Department).LoadAsync();
        return MapItem(item);
    }

    public async Task<ActionPlanItemDto> UpdateItemAsync(int actionPlanId, int itemId, UpdateActionPlanItemRequest request, int userId)
    {
        var item = await db.ActionPlanItems
            .Include(i => i.Department)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ActionPlanId == actionPlanId)
            ?? throw new KeyNotFoundException("ไม่พบรายการ");

        var canEdit = await permissionService.CanEditDepartmentAsync(userId, request.DepartmentId);
        if (!canEdit) throw new UnauthorizedAccessException("ไม่มีสิทธิ์แก้ไขข้อมูลของแผนกนี้");

        item.DepartmentId = request.DepartmentId;
        item.InitiativeName = request.InitiativeName;
        item.ResponsiblePerson = request.ResponsiblePerson;
        item.StartDate = ParseDate(request.StartDate);
        item.EndDate = ParseDate(request.EndDate);
        item.BudgetAllocated = request.BudgetAllocated;
        item.BudgetSpent = request.BudgetSpent;
        item.Status = Enum.Parse<ActionPlanStatus>(request.Status);
        item.Notes = request.Notes;
        item.SortOrder = request.SortOrder;
        item.LastModifiedBy = userId;
        item.LastModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await db.Entry(item).Reference(i => i.Department).LoadAsync();
        return MapItem(item);
    }

    public async Task DeleteItemAsync(int actionPlanId, int itemId, int userId)
    {
        var item = await db.ActionPlanItems.FirstOrDefaultAsync(i => i.Id == itemId && i.ActionPlanId == actionPlanId)
            ?? throw new KeyNotFoundException("ไม่พบรายการ");

        var canEdit = await permissionService.CanEditDepartmentAsync(userId, item.DepartmentId);
        if (!canEdit) throw new UnauthorizedAccessException("ไม่มีสิทธิ์ลบข้อมูลของแผนกนี้");

        db.ActionPlanItems.Remove(item);
        await db.SaveChangesAsync();
    }

    public async Task ReorderItemsAsync(int actionPlanId, ReorderItemsRequest request, int userId)
    {
        var items = await db.ActionPlanItems.Where(i => i.ActionPlanId == actionPlanId).ToListAsync();
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == request.OrderedIds[i]);
            if (item != null) item.SortOrder = i;
        }
        await db.SaveChangesAsync();
    }

    private static DateOnly? ParseDate(string? dateStr) =>
        DateOnly.TryParse(dateStr, out var d) ? d : null;

    private static ActionPlanItemDto MapItem(ActionPlanItem i) =>
        new(i.Id, i.ActionPlanId, i.DepartmentId, i.Department?.Name ?? "",
            i.InitiativeName, i.ResponsiblePerson,
            i.StartDate?.ToString("yyyy-MM-dd"), i.EndDate?.ToString("yyyy-MM-dd"),
            i.BudgetAllocated, i.BudgetSpent, i.Status.ToString(), i.Notes, i.SortOrder, i.LastModifiedAt);
}
