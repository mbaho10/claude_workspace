using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BudgetPlanner.Application.DTOs.Auth;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BudgetPlanner.Application.Services;

public class AuthService(AppDbContext db, IConfiguration config, IPermissionService permissionService) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users
            .Include(u => u.Department)
            .Include(u => u.DepartmentPermissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("อีเมลหรือรหัสผ่านไม่ถูกต้อง");

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var editableIds = await permissionService.GetEditableDepartmentIdsAsync(user.Id);
        var accessToken = GenerateAccessToken(user, editableIds);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResponse(accessToken, refreshToken, MapToProfile(user, editableIds));
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Department)
            .Include(t => t.User).ThenInclude(u => u.DepartmentPermissions)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsActive)
            throw new UnauthorizedAccessException("Refresh token ไม่ถูกต้องหรือหมดอายุ");

        token.RevokedAt = DateTime.UtcNow;

        var editableIds = await permissionService.GetEditableDepartmentIdsAsync(token.User.Id);
        var accessToken = GenerateAccessToken(token.User, editableIds);
        var newRefreshToken = await CreateRefreshTokenAsync(token.User.Id);

        await db.SaveChangesAsync();

        return new AuthResponse(accessToken, newRefreshToken, MapToProfile(token.User, editableIds));
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(int userId)
    {
        var user = await db.Users
            .Include(u => u.Department)
            .Include(u => u.DepartmentPermissions)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive)
            ?? throw new KeyNotFoundException("ไม่พบผู้ใช้");

        var editableIds = await permissionService.GetEditableDepartmentIdsAsync(userId);
        return MapToProfile(user, editableIds);
    }

    private string GenerateAccessToken(User user, List<int> editableDeptIds)
    {
        var secretKey = config["JwtSettings:SecretKey"]!;
        var issuer = config["JwtSettings:Issuer"]!;
        var audience = config["JwtSettings:Audience"]!;
        var minutes = int.Parse(config["JwtSettings:AccessTokenMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("fullName", user.FullName),
            new("deptId", user.DepartmentId?.ToString() ?? ""),
            new("editableDepts", string.Join(",", editableDeptIds))
        };

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var days = int.Parse(config["JwtSettings:RefreshTokenDays"] ?? "7");
        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        });
        await db.SaveChangesAsync();
        return tokenValue;
    }

    private static UserProfileDto MapToProfile(User user, List<int> editableIds) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(),
            user.DepartmentId, user.Department?.Name, editableIds);
}
