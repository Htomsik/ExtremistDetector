using ExtremistDetector.Contracts.Constants;
using ExtremistDetector.Contracts.Models;
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
            
            x.DisableUsageTelemetry();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureQueue();
                cfg.Host(hostName, "/");
                cfg.ConfigureEndpoints(context);
            });
        });
        return services;
    }

    /// <summary>
    ///  Instantly initialize queue and exchanges
    /// </summary>
    private static IRabbitMqBusFactoryConfigurator ConfigureQueue(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Publish<TextContentReport>(p => 
        {
            p.BindQueue(RabbitMqConstants.TextInquisitor, RabbitMqConstants.TextInquisitor);
        });
                
        cfg.Publish<ImageContentReport>(p => 
        {
            p.BindQueue(RabbitMqConstants.ImageInquisitor, RabbitMqConstants.ImageInquisitor);
        });
        
        cfg.Publish<ViolationReport>(p => 
        {
            p.BindQueue(RabbitMqConstants.Butcher, RabbitMqConstants.Butcher);
        });
        
        return cfg;
    }
}