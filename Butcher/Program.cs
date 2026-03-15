using Butcher.Consumers;
using Butcher.Services;
using ExtremistDetector.Infrastructure.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IViolationService, ViolationService>();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddRabbitMQ(builder.Configuration, x =>
{
    x.AddConsumer<ViolationConsumer>();
});

var host = builder.Build();
host.Run();