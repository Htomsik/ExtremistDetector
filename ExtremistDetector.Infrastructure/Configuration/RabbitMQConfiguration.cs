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
                cfg.ConfigureQueue();
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
            options.WaitUntilStarted = false;
            options.StartTimeout = TimeSpan.FromSeconds(2); 
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