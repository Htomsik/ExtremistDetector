using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremistDetector.Infrastructure.Configuration;


public static class RabbitMqConfiguration
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, 
        string hostName = "localhost",
        Action<IBusRegistrationConfigurator> ? configureBus = null)
    {
        services.AddMassTransit(x =>
        {
            configureBus?.Invoke(x);
            
            // This will help to create queue before launching consumer
            x.AddConfigureEndpointsCallback((context, name, cfg) => { });
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(hostName, "/");
                cfg.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}