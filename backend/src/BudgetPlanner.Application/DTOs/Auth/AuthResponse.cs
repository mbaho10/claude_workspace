namespace BudgetPlanner.Application.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserProfileDto User
);

public record UserProfileDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    int? DepartmentId,
    string? DepartmentName,
    List<int> EditableDepartmentIds
);
