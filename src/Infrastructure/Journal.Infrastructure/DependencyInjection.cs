using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Journal.Infrastructure.Persistence;
using Journal.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Journal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o => o
            .UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>()); // same instance per request
        return services;
    }
}