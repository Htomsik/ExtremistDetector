using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ExtremistDetector.Infrastructure.Configuration;

public static class RedisConfiguration
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        var options = ConfigurationOptions.Parse(connectionString);
        
        services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(options));
        
        return services;
    }
}