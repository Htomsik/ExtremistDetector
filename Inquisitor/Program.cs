using ExtremistDetector.Contracts.Models;
using ExtremistDetector.Infrastructure.Configuration;
using Inquisitor.Consumers;
using Inquisitor.Infrastructure;
using Inquisitor.Models;
using Inquisitor.Services;

var builder = Host.CreateApplicationBuilder(args);

// Settings
builder.Services.Configure<OCRSettings>(builder.Configuration.GetSection("OCR"));
builder.Configuration.AddJsonFile("ViolationTextDictionary.json", optional: false);

builder.Services.AddRabbitMQ("localhost", x =>
{
    x.AddConsumer<TextContentConsumer>();
    x.AddConsumer<ImageContentConsumer>();
});

builder.Services.AddSingleton<IOCRProvider, TesseractOCRProvider>(); // SingleTon because OCR is heavy
builder.Services.AddSingleton<IViolationChecker<string>,  ViolationTextChecker>(); // SingleTon because Dictonary is heavy
builder.Services.AddTransient<IViolationChecker<ImageContentReport>,  ViolationImageChecker>();

var host = builder.Build();
await host.Services.EnsureTesseract(); // Init OCR
host.Run();