using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PayCore.Infrastructure.Services;
using Xunit;

namespace PayCore.Tests;

public class TokenServiceTests
{
    private const string SecretKey = "super-secret-key-for-testing-purposes-32chars!!";
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = SecretKey,
                ["Jwt:Issuer"] = "PayCore",
                ["Jwt:Audience"] = "PayCoreClients",
                ["Jwt:ExpiresInMinutes"] = "15"
            })
            .Build();

        _sut = new TokenService(cfg);
    }

    // ── GenerateAccessToken ────────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ContainsSubEmailEmployeeIdClaims()
    {
        var token = _sut.GenerateAccessToken("user-123", "test@example.com", ["Admin"], "emp-001");

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("user-123", parsed.Subject);
        Assert.Equal("test@example.com", parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("emp-001", parsed.Claims.First(c => c.Type == "employeeId").Value);
    }

    [Fact]
    public void GenerateAccessToken_SingleRole_RoleClaimPresent()
    {
        var token = _sut.GenerateAccessToken("u1", "a@b.com", ["Admin"], null);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(parsed.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_MultipleRoles_AllRoleClaimsPresent()
    {
        var token = _sut.GenerateAccessToken("u2", "b@b.com", ["Admin", "Manager"], null);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roles = parsed.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        Assert.Contains("Admin", roles);
        Assert.Contains("Manager", roles);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        var token = _sut.GenerateAccessToken("u3", "c@b.com", ["Employee"], null);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("PayCore", parsed.Issuer);
        Assert.Contains("PayCoreClients", parsed.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresApproximatelyIn15Minutes()
    {
        var token = _sut.GenerateAccessToken("u4", "d@b.com", ["Employee"], null);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.InRange(parsed.ValidTo, DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public void GenerateAccessToken_PassesSignatureValidation()
    {
        var token = _sut.GenerateAccessToken("u5", "e@b.com", ["Admin"], null);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PayCore",
            ValidAudience = "PayCoreClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
        Assert.NotNull(principal);
    }

    [Fact]
    public void GenerateAccessToken_NullEmployeeId_EmptyStringInClaim()
    {
        var token = _sut.GenerateAccessToken("u6", "f@b.com", ["Employee"], null);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(string.Empty, parsed.Claims.First(c => c.Type == "employeeId").Value);
    }

    // ── GenerateRefreshToken ──────────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_IsValidBase64Of64Bytes()
    {
        var token = _sut.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateRefreshToken_EachCallProducesUniqueValue()
    {
        var t1 = _sut.GenerateRefreshToken();
        var t2 = _sut.GenerateRefreshToken();

        Assert.NotEqual(t1, t2);
    }

    // ── HashToken ─────────────────────────────────────────────────────────────

    [Fact]
    public void HashToken_SameInputAlwaysProducesSameOutput()
    {
        var h1 = _sut.HashToken("some-refresh-token");
        var h2 = _sut.HashToken("some-refresh-token");

        Assert.Equal(h1, h2);
    }

    [Fact]
    public void HashToken_DifferentInputsProduceDifferentHashes()
    {
        Assert.NotEqual(_sut.HashToken("token-a"), _sut.HashToken("token-b"));
    }

    [Fact]
    public void HashToken_OutputIs64LowercaseHexChars()
    {
        var hash = _sut.HashToken("any-value");

        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]+$", hash);
    }
}
