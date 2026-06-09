using BudgetPlanner.Application.DTOs.Budget;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Infrastructure.Services;

public class BudgetService(AppDbContext db, IPermissionService permissionService) : IBudgetService
{
    public async Task<List<BudgetTemplateDto>> GetTemplatesAsync(int? year = null)
    {
        var query = db.BudgetTemplates.AsQueryable();
        if (year.HasValue) query = query.Where(t => t.Year == year.Value);
        return await query.OrderByDescending(t => t.Year).ThenBy(t => t.Name)
            .Select(t => MapTemplate(t))
            .ToListAsync();
    }

    public async Task<BudgetTemplateDto> CreateTemplateAsync(CreateBudgetTemplateRequest request, int userId)
    {
        var periodType = Enum.Parse<PeriodType>(request.PeriodType, true);
        var template = new BudgetTemplate
        {
            Name = request.Name,
            Year = request.Year,
            PeriodType = periodType,
            CreatedBy = userId
        };
        db.BudgetTemplates.Add(template);
        await db.SaveChangesAsync();
        return MapTemplate(template);
    }

    public async Task<BudgetTemplateDto> GetTemplateAsync(int templateId)
    {
        var template = await db.BudgetTemplates.FindAsync(templateId)
            ?? throw new KeyNotFoundException("ไม่พบ Template");
        return MapTemplate(template);
    }

    public async Task LockTemplateAsync(int templateId, bool locked)
    {
        var template = await db.BudgetTemplates.FindAsync(templateId)
            ?? throw new KeyNotFoundException("ไม่พบ Template");
        template.IsLocked = locked;
        await db.SaveChangesAsync();
    }

    public async Task<BudgetGridDataResponse> GetGridDataAsync(int templateId, int userId, int? departmentId = null)
    {
        var template = await db.BudgetTemplates.FindAsync(templateId)
            ?? throw new KeyNotFoundException("ไม่พบ Template");

        var isAdmin = await permissionService.IsAdminAsync(userId);
        var editableIds = await permissionService.GetEditableDepartmentIdsAsync(userId);
        var viewableIds = await permissionService.GetViewableDepartmentIdsAsync(userId);

        var deptQuery = db.Departments.Where(d => d.IsActive);
        if (!isAdmin) deptQuery = deptQuery.Where(d => viewableIds.Contains(d.Id));
        if (departmentId.HasValue) deptQuery = deptQuery.Where(d => d.Id == departmentId.Value);

        var departments = await deptQuery.OrderBy(d => d.Name).ToListAsync();
        var deptIds = departments.Select(d => d.Id).ToList();

        var categories = await db.BudgetCategories
            .Where(c => c.TemplateId == templateId && c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .ToListAsync();

        var cells = await db.BudgetCells
            .Where(c => c.TemplateId == templateId && deptIds.Contains(c.DepartmentId))
            .ToListAsync();

        var periodLabels = GetPeriodLabels(template.PeriodType);

        return new BudgetGridDataResponse(
            MapTemplate(template),
            categories.Select(MapCategory).ToList(),
            departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Code)).ToList(),
            periodLabels,
            cells.Select(c => new BudgetCellDto(c.CategoryId, c.DepartmentId, c.PeriodIndex, c.Amount, c.Note)).ToList(),
            editableIds
        );
    }

    public async Task SaveCellsAsync(int templateId, SaveCellsRequest request, int userId)
    {
        var template = await db.BudgetTemplates.FindAsync(templateId)
            ?? throw new KeyNotFoundException("ไม่พบ Template");

        if (template.IsLocked)
            throw new InvalidOperationException("Template นี้ถูกล็อคแล้ว ไม่สามารถแก้ไขได้");

        var editableIds = await permissionService.GetEditableDepartmentIdsAsync(userId);
        var unauthorizedDepts = request.Cells
            .Select(c => c.DepartmentId)
            .Distinct()
            .Where(deptId => !editableIds.Contains(deptId))
            .ToList();

        if (unauthorizedDepts.Count > 0)
            throw new UnauthorizedAccessException($"ไม่มีสิทธิ์แก้ไขข้อมูลของแผนก: {string.Join(", ", unauthorizedDepts)}");

        foreach (var cellReq in request.Cells)
        {
            var existing = await db.BudgetCells.FirstOrDefaultAsync(c =>
                c.TemplateId == templateId &&
                c.CategoryId == cellReq.CategoryId &&
                c.DepartmentId == cellReq.DepartmentId &&
                c.PeriodIndex == cellReq.PeriodIndex);

            if (existing != null)
            {
                existing.Amount = cellReq.Amount;
                existing.Note = cellReq.Note;
                existing.LastModifiedBy = userId;
                existing.LastModifiedAt = DateTime.UtcNow;
            }
            else
            {
                db.BudgetCells.Add(new BudgetCell
                {
                    TemplateId = templateId,
                    CategoryId = cellReq.CategoryId,
                    DepartmentId = cellReq.DepartmentId,
                    PeriodIndex = cellReq.PeriodIndex,
                    Amount = cellReq.Amount,
                    Note = cellReq.Note,
                    LastModifiedBy = userId,
                    LastModifiedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task<BudgetCategoryDto> CreateCategoryAsync(int templateId, CreateCategoryRequest request)
    {
        var category = new BudgetCategory
        {
            TemplateId = templateId,
            Name = request.Name,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            IsCalculated = request.IsCalculated,
            CategoryCode = request.CategoryCode
        };
        db.BudgetCategories.Add(category);
        await db.SaveChangesAsync();
        return MapCategory(category);
    }

    public async Task<BudgetCategoryDto> UpdateCategoryAsync(int templateId, int categoryId, UpdateCategoryRequest request)
    {
        var category = await db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.TemplateId == templateId)
            ?? throw new KeyNotFoundException("ไม่พบหมวดหมู่");

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.IsCalculated = request.IsCalculated;
        category.CategoryCode = request.CategoryCode;
        await db.SaveChangesAsync();
        return MapCategory(category);
    }

    public async Task DeleteCategoryAsync(int templateId, int categoryId)
    {
        var category = await db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.TemplateId == templateId)
            ?? throw new KeyNotFoundException("ไม่พบหมวดหมู่");
        category.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task<byte[]> ExportToExcelAsync(int templateId, int userId)
    {
        var data = await GetGridDataAsync(templateId, userId);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Budget Planning");

        ws.Cell(1, 1).Value = "หมวดหมู่";
        int col = 2;
        foreach (var dept in data.Departments)
        {
            foreach (var period in data.PeriodLabels)
            {
                ws.Cell(1, col).Value = $"{dept.Name} - {period}";
                col++;
            }
            ws.Cell(1, col).Value = $"{dept.Name} - รวม";
            col++;
        }

        var cellMap = data.Cells.ToDictionary(c => $"{c.CategoryId}_{c.DepartmentId}_{c.PeriodIndex}", c => c.Amount);
        int row = 2;
        foreach (var cat in data.Categories)
        {
            ws.Cell(row, 1).Value = cat.Name;
            col = 2;
            foreach (var dept in data.Departments)
            {
                decimal deptTotal = 0;
                for (byte p = 0; p < data.PeriodLabels.Count; p++)
                {
                    var amount = cellMap.GetValueOrDefault($"{cat.Id}_{dept.Id}_{p}", 0);
                    ws.Cell(row, col).Value = amount;
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                    deptTotal += amount;
                    col++;
                }
                ws.Cell(row, col).Value = deptTotal;
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                col++;
            }
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.Row(1).Style.Font.Bold = true;
        ws.SheetView.FreezeRows(1);
        ws.SheetView.FreezeColumns(1);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static List<string> GetPeriodLabels(PeriodType type) => type == PeriodType.Monthly
        ? ["ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค."]
        : ["ไตรมาส 1", "ไตรมาส 2", "ไตรมาส 3", "ไตรมาส 4"];

    private static BudgetTemplateDto MapTemplate(BudgetTemplate t) =>
        new(t.Id, t.Name, t.Year, t.PeriodType.ToString(),
            t.PeriodType == PeriodType.Monthly ? 12 : 4, t.IsLocked, t.CreatedAt);

    private static BudgetCategoryDto MapCategory(BudgetCategory c) =>
        new(c.Id, c.Name, c.ParentId, c.SortOrder, c.IsCalculated, c.CategoryCode);
}
