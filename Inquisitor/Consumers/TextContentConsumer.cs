using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Inquisitor.Consumers;

public class TextContentConsumer : IConsumer<TextContentReport>
{
    readonly ILogger<TextContentConsumer> _logger;

    public TextContentConsumer(ILogger<TextContentConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<TextContentReport> context)
    {
        var originalContent = context.Message;

        
        return Task.CompletedTask;
    }
}