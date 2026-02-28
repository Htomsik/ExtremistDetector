using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Inquisitor.Consumers;

public class ImageContentConsumer : IConsumer<ImageContentReport>
{
    readonly ILogger<ImageContentConsumer> _logger;

    public ImageContentConsumer(ILogger<ImageContentConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<ImageContentReport> context)
    {
        var originalContent = context.Message;

        
        return Task.CompletedTask;
    }
}