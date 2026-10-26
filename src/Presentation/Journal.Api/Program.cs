using System.Text;
using System.Text.Json.Serialization;
using Journal.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Journal.Infrastructure;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Add Controllers & prevent JSON looping
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// 3. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    // Tells ASP.NET Core to look for a Bearer token on all secured endpoints
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // This must match the exact same key used in IdentityService.cs
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// 4. Configure the HTTP Request Pipeline
// CRITICAL: Authentication must happen before Authorization, and both must be before MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

