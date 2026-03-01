using ExtremistDetector.Infrastructure.Configuration;
using Inquisitor.Consumers;
using Inquisitor.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("ViolationTextDictionary.json", optional: false);
builder.Services.AddRabbitMQ("localhost", x =>
{
    x.AddConsumer<TextContentConsumer>();
    x.AddConsumer<ImageContentConsumer>();
});
builder.Services.AddSingleton<IViolationChecker<string>,  ViolationTextChecker>();
var host = builder.Build();
host.Run();