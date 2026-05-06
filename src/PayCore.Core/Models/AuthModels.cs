namespace PayCore.Core.Models;

public record RegisterRequest(
    string Email,
    string Password,
    string Role,
    string? DepartmentId = null,
    string? EmployeeId = null);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry);

public record RefreshRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);
