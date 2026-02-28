using ExtremistDetector.Infrastructure.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitMQ();

var host = builder.Build();
host.Run();