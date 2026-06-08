using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department? Department { get; set; }
    public ICollection<UserDepartmentPermission> DepartmentPermissions { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
