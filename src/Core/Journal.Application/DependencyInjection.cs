using Microsoft.Extensions.DependencyInjection;

namespace Journal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> devin/1792968709-step2-layering
