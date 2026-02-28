using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Butcher.Consumers;

public class ViolationConsumer : IConsumer<ViolationReport>
{
    readonly ILogger<ViolationConsumer> _logger;

    public ViolationConsumer(ILogger<ViolationConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<ViolationReport> context)
    {
        return Task.CompletedTask;
    }
}