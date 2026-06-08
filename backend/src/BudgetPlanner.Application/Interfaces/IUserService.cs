using BudgetPlanner.Application.DTOs.User;

namespace BudgetPlanner.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<List<UserPermissionDto>> GetUserPermissionsAsync(int userId);
    Task SetUserPermissionsAsync(int userId, SetPermissionsRequest request);
}
