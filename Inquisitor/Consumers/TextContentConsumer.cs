using ExtremistDetector.Contracts.Models;
using Inquisitor.Services;
using MassTransit;

namespace Inquisitor.Consumers;

public class TextContentConsumer : IConsumer<TextContentReport>
{
    readonly ILogger<TextContentConsumer> _logger;
    
    private readonly IViolationChecker<string> _violationChecker;
    
    private readonly IBus _bus;
    
    public TextContentConsumer(ILogger<TextContentConsumer> logger, IViolationChecker<string> violationChecker, IBus bus)
    {
        _logger = logger;
        _violationChecker = violationChecker;
        _bus = bus;
    }
    
    public async Task Consume(ConsumeContext<TextContentReport> context)
    {
       var contentReport = context.Message;
       var startProcessTime = DateTime.UtcNow;
       
       _logger.LogInformation("{ContentId} processing..", contentReport.ContentId);
       
       if (string.IsNullOrEmpty(contentReport.Content))
       {
           _logger.LogWarning("{ContentId} Content not founded.", contentReport.ContentId);
           return;
       }
       
       var result = await _violationChecker.Check(contentReport.Content, context.CancellationToken);
       if (result.Type == ViolationType.None)
       {
           _logger.LogInformation("{ContentId} processed. No violations", contentReport.ContentId);
           return;
       }
       
       //TODO In ideal add mapper 
       var violationReport = new ViolationReport(
           contentReport.ContentId,
           contentReport.ContentType,
           result.Type, 
           contentReport.Source, 
           contentReport.Content, 
           result.TriggeredData,
           contentReport.CreatedTime,
           startProcessTime,
           DateTime.UtcNow);

       if (context.CancellationToken.IsCancellationRequested)
       {
           _logger.LogInformation("{ContentId} canceled", contentReport.ContentId);
           return;
       }
       
       await _bus.Publish(violationReport);
       
       _logger.LogInformation("{ContentId} processed. Found violations", contentReport.ContentId);
    }
}