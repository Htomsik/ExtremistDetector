using ExtremistDetector.Contracts.Models;
using Informer.Models;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Informer;

public class Worker : BackgroundService
{
    private readonly IBusControl _bus;
    private readonly ILogger<Worker> _logger;
    private readonly HealthCheckService _healthCheckService;

    private readonly Settings _settings;
    private List<Task>? _publishTasks;

    public Worker(ILogger<Worker> logger, IBusControl bus, IOptions<Settings> settings, HealthCheckService healthCheckService)
    {
        _logger = logger;
        _bus = bus;
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
            var health = _bus.CheckHealth();
            if (health.Status != BusHealthStatus.Healthy)
            {
                _logger.LogWarning("Bus is unavailable, waiting...");
                await Task.Delay(2000, stoppingToken); 
                continue;
            }
            
            var processId = Guid.NewGuid();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var files = Directory.GetFiles(_settings.WorkDirectory)
                .Where(x => _settings.AllSupportedFormats.Contains(Path.GetExtension(x))).ToList();

            if (files.Count != 0)
            {
                _logger.LogInformation("{processId} Processing {count} files...", processId, files.Count());
            }
            
            _publishTasks = new List<Task>();
            foreach (var file in files)
            {
                if (!IsFileReady(file))
                    continue;
                
                try
                {
                    await  ProcessFile(file, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError("{processId} Unexpected error in loop: {Message}", processId, e.Message);
                    continue;
                }
                
                if (_publishTasks.Count > 100) // More than 100 bus stattering
                {
                    await Task.WhenAll(_publishTasks);
                    _publishTasks.Clear();
                }
            }
            await Task.WhenAll(_publishTasks);

            watch.Stop();
            if (files.Count != 0)
            {
                var totalSeconds = watch.Elapsed.TotalSeconds;
                var filesPerSecond = totalSeconds > 0 ? Math.Round(files.Count / totalSeconds, 2) : 0;

                _logger.LogInformation("{processId} {count} files processed by {ms} m. Speed: {speed} f/sec", 
                    processId, files.Count, watch.ElapsedMilliseconds, filesPerSecond);
            }
            
            // in real projects use FileSystemWatcher instead of this 
            await Task.Delay(1000 * 5, stoppingToken);
        }
    }
    
    // In real project just take it in service, but there is no need
    private async Task ProcessFile(string filePath, CancellationToken stoppingToken)
    {
        // In Real project we need to store files on shared directory
        var fileName = Path.GetFileName(filePath);
        var archiveFilePath = Path.Combine(_settings.FullArchiveDirectory, fileName);
        var ext = Path.GetExtension(filePath);

        var contentType = ContentType.Unknown;

        if (_settings.TextSupportedFormats.Contains(ext))
            contentType = ContentType.Text;
        else if (_settings.ImageSupportedFormats.Contains(ext))
            contentType = ContentType.Image;
        
        var reportId = Guid.NewGuid();
        
        switch (contentType)
        {
            case ContentType.Text:
                var textReport = new TextContentReport(
                    reportId,
                    contentType,
                    fileName,
                    await File.ReadAllTextAsync(filePath, stoppingToken),
                    DateTime.UtcNow);
                
                File.Move(filePath, archiveFilePath, true);
                _publishTasks.Add(_bus.Publish(textReport, stoppingToken));
                break;

            case ContentType.Image:
                var imageReport = new ImageContentReport(
                    reportId,
                    contentType,
                    fileName,
                    archiveFilePath,
                    DateTime.UtcNow);
                
                File.Move(filePath, archiveFilePath, true);
                _publishTasks.Add(_bus.Publish(imageReport, stoppingToken));
                break;
        }
    }

    private bool IsFileReady(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
                return false;
            
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); // Low cost checking 
        }
        catch (Exception e)
        {
            return false;
        }
        
        return true;
    }
}