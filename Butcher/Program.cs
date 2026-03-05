using Butcher.Consumers;
using ExtremistDetector.Infrastructure.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitMQ(builder.Configuration, x =>
{
    x.AddConsumer<ViolationConsumer>();
});

var host = builder.Build();
host.Run();