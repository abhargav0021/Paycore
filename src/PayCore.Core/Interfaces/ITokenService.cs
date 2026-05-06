namespace PayCore.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, IList<string> roles, string? employeeId);
    string GenerateRefreshToken();
    string HashToken(string token);
}
