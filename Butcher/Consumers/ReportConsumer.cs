using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Butcher.Consumers;

public class ReportConsumer : IConsumer<Report>
{
    readonly ILogger<ReportConsumer> _logger;

    public ReportConsumer(ILogger<ReportConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<Report> context)
    {
        return Task.CompletedTask;
    }
}