using System.ComponentModel.DataAnnotations;

namespace Journal.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Result, string? Token, string? Error)> RegisterUserAsync(string email, string password);
    Task<(bool Result, string? Token, string? Error)> LoginUserAsync(string email, string password);
}