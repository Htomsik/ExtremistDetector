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
    
    public async Task Consume(ConsumeContext<ImageContentReport> context)
    {
        var contentReport = context.Message;
        var startProcessTime = DateTime.UtcNow;
       
        _logger.LogInformation("{ContentId} processing..", contentReport.ContentId);
       
        if (string.IsNullOrEmpty(contentReport.Url))
        {
            _logger.LogWarning("{ContentId} Content not founded.", contentReport.ContentId);
            return;
        }
       
        var result = await _violationChecker.Check(contentReport, context.CancellationToken);
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
            contentReport.Url, 
            result.TriggeredData,
            contentReport.CreatedTime,
            startProcessTime,
            DateTime.UtcNow
            );
       
        if (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("{ContentId} canceled", contentReport.ContentId);
            return;
        }
        
        await _bus.Publish(violationReport);
       
        _logger.LogInformation("{ContentId} processed. Found violations", contentReport.ContentId);
    }
}