using BudgetPlanner.Application.DTOs.Auth;

namespace BudgetPlanner.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserProfileDto> GetCurrentUserAsync(int userId);
}
