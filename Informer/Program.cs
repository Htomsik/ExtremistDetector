using ExtremistDetector.Infrastructure.Configuration;
using Informer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddRabbitMQ();

var host = builder.Build();
host.Run();