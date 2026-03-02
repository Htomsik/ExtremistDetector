using ExtremistDetector.Contracts.Models;
using Inquisitor.Services;
using MassTransit;

namespace Inquisitor.Consumers;

public class ImageContentConsumer : IConsumer<ImageContentReport>
{
    private readonly IViolationChecker<ImageContentReport> _violationChecker;
    private readonly IBus _bus;
    readonly ILogger<ImageContentConsumer> _logger;

    public ImageContentConsumer(ILogger<ImageContentConsumer> logger,
        IViolationChecker<ImageContentReport> imageViolationChecker,
        IBus bus)
    {
        _violationChecker = imageViolationChecker;
        _bus = bus;
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<ImageContentReport> context)
    {
        var contentReport = context.Message;
       
        _logger.LogInformation("{ContentId} processing..", contentReport.ContentId);
       
        if (string.IsNullOrEmpty(contentReport.Url))
        {
            _logger.LogWarning("{ContentId} Content not founded.", contentReport.ContentId);
            return Task.CompletedTask;
        }
       
        var violationType = _violationChecker.Check(contentReport);
        if (violationType == ViolationType.None)
        {
            _logger.LogInformation("{ContentId} processed. No violations", contentReport.ContentId);
            return Task.CompletedTask;
        }
       
        //TODO In ideal add mapper 
        var violationReport = new ViolationReport(violationType, 
            contentReport.Source, 
            contentReport.Url, 
            contentReport.CreatedTime);
       
        _bus.Publish(violationReport);
       
        _logger.LogInformation("{ContentId} processed. Found violations", contentReport.ContentId);

        return Task.CompletedTask;
        
        return Task.CompletedTask;
    }
}