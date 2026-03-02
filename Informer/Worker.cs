using ExtremistDetector.Contracts.Models;
using Informer.Models;
using MassTransit;

namespace Informer;

public class Worker : BackgroundService
{
    private readonly IBus _bus;
    private readonly ILogger<Worker> _logger;

    private readonly Settings _settings;

    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
        _settings = new Settings();
    }

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Created only if doesn't exist
        Directory.CreateDirectory(_settings.WorkDirectory);
        Directory.CreateDirectory(_settings.ArchiveDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processId = Guid.NewGuid();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var files = Directory.GetFiles(_settings.WorkDirectory)
                .Where(x => _settings.AllSupportedFormats.Contains(Path.GetExtension(x))).ToList();

            if (files.Count != 0)
            {
                _logger.LogInformation("{processId} Processing {count} files...", processId, files.Count());
            }
            
            foreach (var file in files)
            {
                await ProcessFile(file, stoppingToken);
            }

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
        var archiveFilePath = Path.Combine(_settings.ArchiveDirectory, fileName);
        var ext = Path.GetExtension(filePath);

        var contentType = ContentType.Unknown;

        if (_settings.TextSupportedFormats.Contains(ext))
            contentType = ContentType.Text;
        else if (_settings.ImageSupportedFormats.Contains(ext))
            contentType = ContentType.Image;

        IContentReport? report = null;
        var reportId = Guid.NewGuid();
        
        switch (contentType)
        {
            case ContentType.Text:
                report = new TextContentReport(
                    reportId,
                    fileName,
                    await File.ReadAllTextAsync(filePath, stoppingToken),
                    DateTime.UtcNow);

                await _bus.Publish((TextContentReport)report, stoppingToken);
                break;

            case ContentType.Image:
                report = new ImageContentReport(
                    reportId,
                    fileName,
                    archiveFilePath,
                    DateTime.UtcNow);

                await _bus.Publish((ImageContentReport)report, stoppingToken);
                break;
        }

        if (report == null)
            return;

        File.Move(filePath, archiveFilePath, true);
       
    }
}