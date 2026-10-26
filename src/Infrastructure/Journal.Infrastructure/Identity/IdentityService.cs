using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Journal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Journal.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public IdentityService(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<(bool Result, string? Token, string? Error)> RegisterUserAsync(string email, string password)
    {
        var user = new IdentityUser { UserName = email, Email = email };

        // CreateAsync automatically hashes the password using modern cryptographic standards
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, null, error);
        }

        var token = GenerateJwtToken(user);
        return (true, token, null);
    }

    public async Task<(bool Result, string? Token, string? Error)> LoginUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return (false, null, "Invalid credentials");

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!validPassword) return (false, null, "Invalid credentials");

        var token = GenerateJwtToken(user);
        return (true, token, null);
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

}