using System.Text.Json;
using Butcher.Services;
using ExtremistDetector.Contracts.Models;
using MassTransit;
using StackExchange.Redis;

namespace Butcher.Consumers;

public class ViolationConsumer : IConsumer<ViolationReport>
{
    readonly ILogger<ViolationConsumer> _logger;
    
    private readonly IViolationService _violationService;

    public ViolationConsumer(ILogger<ViolationConsumer> logger, IViolationService violationService)
    {
        _logger = logger;
        _violationService = violationService;
    }
    
    public async Task Consume(ConsumeContext<ViolationReport> context)
    {
        var contentReport = context.Message;
        
        _logger.LogInformation("{Source} processing..", contentReport.Source);

        await _violationService.Resolve(contentReport);
        
        _logger.LogInformation("{Source} processed", contentReport.Source);
    }
}