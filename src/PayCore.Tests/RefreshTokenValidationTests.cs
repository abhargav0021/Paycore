using PayCore.Infrastructure.Identity;
using Xunit;

namespace PayCore.Tests;

public class RefreshTokenValidationTests
{
    private static RefreshToken ActiveToken() => new()
    {
        UserId = "user-123",
        TokenHash = "abc123",
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        IsRevoked = false
    };

    // ── IsValid ───────────────────────────────────────────────────────────────

    [Fact]
    public void IsValid_ActiveUnexpiredToken_ReturnsTrue()
    {
        Assert.True(RefreshTokenValidator.IsValid(ActiveToken()));
    }

    [Fact]
    public void IsValid_RevokedToken_ReturnsFalse()
    {
        var token = ActiveToken();
        token.IsRevoked = true;

        Assert.False(RefreshTokenValidator.IsValid(token));
    }

    [Fact]
    public void IsValid_ExpiredToken_ReturnsFalse()
    {
        var token = ActiveToken();
        token.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);

        Assert.False(RefreshTokenValidator.IsValid(token));
    }

    [Fact]
    public void IsValid_RevokedAndExpiredToken_ReturnsFalse()
    {
        var token = ActiveToken();
        token.IsRevoked = true;
        token.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        Assert.False(RefreshTokenValidator.IsValid(token));
    }

    [Fact]
    public void IsValid_ExpiresExactlyNow_ReturnsFalse()
    {
        var token = ActiveToken();
        token.ExpiresAt = DateTime.UtcNow;

        Assert.False(RefreshTokenValidator.IsValid(token));
    }

    // ── GetInvalidReason ──────────────────────────────────────────────────────

    [Fact]
    public void GetInvalidReason_ValidToken_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, RefreshTokenValidator.GetInvalidReason(ActiveToken()));
    }

    [Fact]
    public void GetInvalidReason_RevokedToken_ContainsRevokedKeyword()
    {
        var token = ActiveToken();
        token.IsRevoked = true;

        Assert.Contains("revoked", RefreshTokenValidator.GetInvalidReason(token), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetInvalidReason_ExpiredToken_ContainsExpiredKeyword()
    {
        var token = ActiveToken();
        token.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);

        Assert.Contains("expired", RefreshTokenValidator.GetInvalidReason(token), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetInvalidReason_RevokedCheckedBeforeExpiry()
    {
        // Both revoked and expired — revoked reason should win (checked first)
        var token = ActiveToken();
        token.IsRevoked = true;
        token.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        Assert.Contains("revoked", RefreshTokenValidator.GetInvalidReason(token), StringComparison.OrdinalIgnoreCase);
    }
}
