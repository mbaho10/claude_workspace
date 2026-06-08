namespace BudgetPlanner.Application.DTOs.User;

public record UserDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    int? DepartmentId,
    string? DepartmentName,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string Email,
    string FullName,
    string Password,
    string Role,
    int? DepartmentId
);

public record UpdateUserRequest(
    string FullName,
    string Role,
    int? DepartmentId,
    bool IsActive
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UserPermissionDto(int DepartmentId, string DepartmentName, string PermissionLevel);

public record SetPermissionsRequest(List<DepartmentPermissionRequest> Permissions);

public record DepartmentPermissionRequest(int DepartmentId, string PermissionLevel);
