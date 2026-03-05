using ExtremistDetector.Infrastructure.Configuration;
using Informer;
using Informer.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.AddHostedService<Worker>();
builder.Services.AddRabbitMQ(builder.Configuration);

var host = builder.Build();
host.Run();