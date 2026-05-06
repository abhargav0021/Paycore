using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayCore.Core.Interfaces;
using PayCore.Core.Models;
using PayCore.Infrastructure.Data;
using PayCore.Infrastructure.Identity;

namespace PayCore.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (!RoleSeeder.Roles.Contains(request.Role))
            throw new ArgumentException($"Role must be one of: {string.Join(", ", RoleSeeder.Roles)}.");

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DepartmentId = request.DepartmentId,
            EmployeeId = request.EmployeeId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        return await IssueTokenPairAsync(user, roles, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        return await IssueTokenPairAsync(user, roles, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = _tokenService.HashToken(refreshToken);

        var stored = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!RefreshTokenValidator.IsValid(stored))
            throw new UnauthorizedAccessException(RefreshTokenValidator.GetInvalidReason(stored));

        // Rotate: mark current as revoked before issuing a new pair
        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(stored.User);
        // IssueTokenPairAsync calls SaveChangesAsync, which persists both the
        // revoked state on the old token and the new token in one round-trip.
        return await IssueTokenPairAsync(stored.User, roles, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = _tokenService.HashToken(refreshToken);

        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored is null || stored.IsRevoked) return;

        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<AuthResponse> IssueTokenPairAsync(AppUser user, IList<string> roles, CancellationToken ct)
    {
        var rawRefresh = _tokenService.GenerateRefreshToken();
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles, user.EmployeeId);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync(ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: rawRefresh,
            AccessTokenExpiry: DateTime.UtcNow.AddMinutes(15));
    }
}
