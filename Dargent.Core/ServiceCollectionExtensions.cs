using Microsoft.Extensions.DependencyInjection;

namespace Dargent.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDargentCore(this IServiceCollection services)
    {
        services.AddTransient<AgentSessionFactory>();

        return services;
    }
}