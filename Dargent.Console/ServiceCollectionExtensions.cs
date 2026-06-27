using Microsoft.Extensions.DependencyInjection;

namespace Dargent.Console;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDargentConsole(this IServiceCollection services)
    {
        services.AddTransient<Session>();
        return services;
    }
}