using ExtremistDetector.Contracts.Models;
using Informer.Models;
using Informer.Services;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Informer;

public class Worker : BackgroundService
{
    private readonly IBusControl _bus;
    private readonly ILogger<Worker> _logger;
    private readonly IFileService<IReport> _fileService;
    private readonly HealthCheckService _healthCheckService;

    private readonly Settings _settings;

    public Worker(ILogger<Worker> logger, 
        IBusControl bus,
        IOptions<Settings> settings,
        IFileService<IReport> fileService,
        HealthCheckService healthCheckService)
    {
        _logger = logger;
        _bus = bus;
        _fileService = fileService;
        _healthCheckService = healthCheckService;
        _settings = settings.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Created only if doesn't exist
        Directory.CreateDirectory(_settings.FullWorkDirectory);
        Directory.CreateDirectory(_settings.FullArchiveDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000 * 5, stoppingToken);
            
            var health = _bus.CheckHealth();
            if (health.Status != BusHealthStatus.Healthy)
            {
                _logger.LogWarning("Bus is unavailable, waiting...");
                await Task.Delay(2000, stoppingToken); 
                continue;
            }
            
            var processId = Guid.NewGuid();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var deletedCount = _fileService.CleanGarbageFiles(_settings.WorkDirectory);
            if (deletedCount != 0)
            {
                _logger.LogInformation("{processId} {count} garbage files deleted.", 
                    processId, deletedCount); 
            }
            
            // TODO in real app add quarantin files  
            var files = Directory.GetFiles(_settings.WorkDirectory);
            var supportedFiles = files
                .Where(x => _settings.AllSupportedFormats.Contains(Path.GetExtension(x))).ToList();
            
            if(supportedFiles.Count == 0)
                continue;
            
            _logger.LogInformation("{processId} Processing {count} files...", processId, supportedFiles.Count());
            
            var publishTasks = new List<Task>();
            foreach (var file in supportedFiles)
            {
                if (!_fileService.IsFileReady(file))
                    continue;
                try
                {
                    var report = await _fileService.ProcessFileAsync(file, stoppingToken);
                    if (report != null)
                    {
                        publishTasks.Add(_bus.Publish(report, report.GetType(),  stoppingToken));
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("{processId} Unexpected error in processing files: {Message}", processId, e.Message);
                    continue;
                }
                
                if (publishTasks.Count > 100) // More than 100 bus stattering
                {
                    await Task.WhenAll(publishTasks);
                    publishTasks.Clear();
                }
            }
            await Task.WhenAll(publishTasks);

            // TODO in ideal move this to redis 
            watch.Stop();
            var totalSeconds = watch.Elapsed.TotalSeconds;
            var filesPerSecond = totalSeconds > 0 ? Math.Round(supportedFiles.Count / totalSeconds, 2) : 0;

            _logger.LogInformation("{processId} {count} files processed by {ms} m. Speed: {speed} f/sec", 
                processId, supportedFiles.Count, watch.ElapsedMilliseconds, filesPerSecond);
        }
    }
}