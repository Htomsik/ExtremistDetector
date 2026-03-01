using ExtremistDetector.Contracts.Models;
using MassTransit;

namespace Informer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    
    private readonly IBus _bus;

    private readonly Settings _settings;
    
    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
        _settings  = new Settings(); 
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Created only if doesn't exist
        Directory.CreateDirectory(_settings.WorkDirectory);
        Directory.CreateDirectory(_settings.ArchiveDirectory);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var files = Directory.GetFiles(_settings.WorkDirectory).Where(x=> _settings.AllSupportedFormats.Contains(Path.GetExtension(x)));
            foreach (var filePath in files)
            {
                // In Real project we need to store files on shared directory
                // But it's just a project for checking microservices
                var fileName = Path.GetFileName(filePath);
                var archiveFilePath = Path.Combine(_settings.ArchiveDirectory, fileName);
                var ext = Path.GetExtension(filePath);

                var contentType = ContentType.Unknown;
            
                if (_settings.TextSupportedFormats.Contains(ext))
                    contentType = ContentType.Text;
                else if (_settings.ImageSupportedFormats.Contains(ext))
                    contentType = ContentType.Image;

                IContentReport? report = null;

                switch (contentType)
                {
                    case ContentType.Text:
                        report = new TextContentReport(
                            fileName,
                            await File.ReadAllTextAsync(filePath, stoppingToken),
                            DateTime.UtcNow);
                        
                        await _bus.Publish((TextContentReport)report, stoppingToken);
                        break;
                    
                    case ContentType.Image:
                        report = new ImageContentReport(
                            fileName,
                            archiveFilePath,  
                            DateTime.UtcNow);
                        
                        await _bus.Publish((ImageContentReport)report, stoppingToken);
                        break;
                }
                
                if (report == null)
                    continue;
                
                File.Move(filePath, archiveFilePath, overwrite: true);
                _logger.LogInformation("File {FileName} ({Type}) processed", fileName, contentType);
            }
        }
    }
}