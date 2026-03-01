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
    
    public Task Consume(ConsumeContext<TextContentReport> context)
    {
       var contentReport = context.Message;
       
       _logger.LogInformation("{ContentId} processing..", contentReport.ContentId);
       
       if (string.IsNullOrEmpty(contentReport.Content))
       {
           _logger.LogWarning("{ContentId} Content not founded.", contentReport.ContentId);
           return Task.CompletedTask;
       }
       
       var violationType = _violationChecker.Check(contentReport.Content);
       if (violationType == ViolationType.None)
       {
           _logger.LogInformation("{ContentId} processed. No violations", contentReport.ContentId);
           return Task.CompletedTask;
       }
       
       //TODO In ideal add mapper 
       var violationReport = new ViolationReport(violationType, 
           contentReport.Source, 
           contentReport.Content, 
           contentReport.CreatedTime);
       
       _bus.Publish(violationReport);
       
       _logger.LogInformation("{ContentId} processed. Found violations", contentReport.ContentId);

       return Task.CompletedTask;
    }
}