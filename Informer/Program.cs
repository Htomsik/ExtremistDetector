using ExtremistDetector.Contracts.Models;
using ExtremistDetector.Infrastructure.Configuration;
using Informer;
using Informer.Models;
using Informer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.AddHostedService<Worker>();
builder.Services.AddRabbitMQ(builder.Configuration);

builder.Services.AddTransient<IFileService<IReport>, ReportFileService>(); 

var host = builder.Build();
host.Run();