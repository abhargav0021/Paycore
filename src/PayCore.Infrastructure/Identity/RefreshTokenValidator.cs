namespace PayCore.Infrastructure.Identity;

public static class RefreshTokenValidator
{
    public static bool IsValid(RefreshToken token) =>
        !token.IsRevoked && token.ExpiresAt > DateTime.UtcNow;

    public static string GetInvalidReason(RefreshToken token)
    {
        if (token.IsRevoked) return "Refresh token has been revoked.";
        if (token.ExpiresAt <= DateTime.UtcNow) return "Refresh token has expired.";
        return string.Empty;
    }
}
