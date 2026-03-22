using ExtremistDetector.Contracts.Constants;
using ExtremistDetector.Contracts.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremistDetector.Infrastructure.Configuration;


public static class RabbitMqConfiguration
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> ? configureBus = null)
    {
        var hostName = configuration["RabbitMQ:Host"] ?? "localhost";
        var userName = configuration["RabbitMQ:Username"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        
        services.AddMassTransit(x =>
        {
            configureBus?.Invoke(x);
            x.DisableUsageTelemetry();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(hostName, "/", host =>
                {
                    host.Username(userName);
                    host.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = true;
            options.StartTimeout = TimeSpan.FromSeconds(10); 
        });
        
        return services;
    }
}