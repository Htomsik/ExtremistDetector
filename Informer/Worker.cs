using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Informer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    
    private readonly IBus _bus;

    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var sourceName = "SomeFile";
            var contentType = ContentType.text;
            var content = "";
            
            var newReport = new ContentReport(sourceName,contentType,content, DateTime.Now); 
            
            await _bus.Publish(newReport, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}