namespace MedicalCenter.Infrastructure.Identity;

/// <summary>
/// Entity for storing refresh tokens for JWT authentication.
/// </summary>
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Revoked { get; set; }
}

