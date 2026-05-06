using PayCore.Core.Entities;

namespace PayCore.Infrastructure.Identity;

public class RefreshToken : BaseEntity
{
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
