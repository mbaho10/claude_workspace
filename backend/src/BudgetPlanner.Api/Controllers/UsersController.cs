using System.Security.Claims;
using BudgetPlanner.Application.DTOs.User;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await userService.GetAllUsersAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        => Ok(await userService.CreateUserAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        => Ok(await userService.UpdateUserAsync(id, request));

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(int id)
        => Ok(await userService.GetUserPermissionsAsync(id));

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetPermissionsRequest request)
    {
        await userService.SetUserPermissionsAsync(id, request);
        return NoContent();
    }
}
