using System.ComponentModel.DataAnnotations;

namespace Journal.Infrastructure.Identity;

public class RefreshToken
{
    [Key]
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
}