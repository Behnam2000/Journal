using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Journal.Application.Common.Interfaces;
using Journal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Journal.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public IdentityService(UserManager<IdentityUser> userManager, IConfiguration configuration, AppDbContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<(bool Result, string? Token, string? RefreshToken, string? Error)> RegisterUserAsync(string email, string password)
    {
        var user = new IdentityUser { UserName = email, Email = email };

        // CreateAsync automatically hashes the password using modern cryptographic standards
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return (false, null, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return await GenerateTokenPairAsync(user);
    }

    public async Task<(bool Result, string? Token, string? RefreshToken, string? Error)> LoginUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return (false, null, null, "Invalid credentials");


        return await GenerateTokenPairAsync(user);
    }

    public async Task<(bool Result, string? Token, string? RefreshToken, string? Error)> RefreshTokenAsync(string expiredToken, string refreshToken)
    {
        // Extract the user ID from the expired JWT
        var principle = GetPrincipalFromExpiredToken(expiredToken);
        if (principle == null) return (false, null, null, "Invalid access token");

        var userId = principle.FindFirstValue(ClaimTypes.NameIdentifier);

        // Validate the refresh token against the database
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == userId);

        if (storedToken == null) return (false, null, null, "Refresh token does not exist");
        if (DateTime.UtcNow > storedToken.ExpiryDate) return (false, null, null, "Refresh token has expired");
        if (storedToken.IsRevoked) return (false, null, null, "Refresh token is revoked");

        // Revoke the old token and issue a new pair
        storedToken.IsRevoked = true;
        _context.RefreshTokens.Update(storedToken);

        var user = await _userManager.FindByIdAsync(userId!);
        return await GenerateTokenPairAsync(user!);

    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked)
            return false; // Token doesen't exist or already revoked

        storedToken.IsRevoked = true;
        _context.RefreshTokens.Update(storedToken);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<(bool Result, string? Token, string? RefreshToken, string? Error)> GenerateTokenPairAsync(IdentityUser user)
    {
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshTokenString();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(20)
        });

        await _context.SaveChangesAsync();
        return (true, jwtToken, refreshToken, null);
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
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false //CRITICAL: Tell the validator to ignore the expiration date so we can read it
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}