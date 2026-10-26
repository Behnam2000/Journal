<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Journal.Infrastructure.Persistence;
using Journal.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
=======
using Journal.Application.Common.Interfaces;
using Journal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
>>>>>>> devin/1792968709-step2-layering

namespace Journal.Infrastructure;

public static class DependencyInjection
{
<<<<<<< HEAD
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o => o
            .UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>()); // same instance per request
        return services;
    }
}
=======
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

        // Resolve the interface to the same scoped AppDbContext instance.
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
>>>>>>> devin/1792968709-step2-layering
