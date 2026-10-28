using System.ComponentModel.DataAnnotations;

namespace Journal.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Result, string? Token, string? RefreshToken, string? Error)> RegisterUserAsync(string email, string password);
    Task<(bool Result, string? Token, string? RefreshToken, string? Error)> LoginUserAsync(string email, string password);

    // method to handle token rotation
    Task<(bool Result, string? Token, string? RefreshToken, string? Error)> RefreshTokenAsync(string expiredToken, string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}