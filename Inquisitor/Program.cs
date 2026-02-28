using ExtremistDetector.Infrastructure.Configuration;
using Inquisitor.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitMQ("localhost", x =>
{
    x.AddConsumer<TextContentConsumer>();
    x.AddConsumer<ImageContentConsumer>();
});

var host = builder.Build();
host.Run();