using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var depts = await db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name, d.Code }).ToListAsync();
        return Ok(depts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        if (await db.Departments.AnyAsync(d => d.Code == request.Code))
            return BadRequest("รหัสแผนกนี้มีในระบบแล้ว");

        var dept = new Department { Name = request.Name, Code = request.Code };
        db.Departments.Add(dept);
        await db.SaveChangesAsync();
        return Ok(new { dept.Id, dept.Name, dept.Code });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDepartmentRequest request)
    {
        var dept = await db.Departments.FindAsync(id);
        if (dept == null) return NotFound();
        dept.Name = request.Name;
        dept.Code = request.Code;
        await db.SaveChangesAsync();
        return Ok(new { dept.Id, dept.Name, dept.Code });
    }
}

public record CreateDepartmentRequest(string Name, string Code);
