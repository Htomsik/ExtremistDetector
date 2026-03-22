using ExtremistDetector.Contracts.Models;
using Informer.Models;
using Microsoft.Extensions.Options;

namespace Informer.Services;

public interface IFileService<T>
{
    Task<T?> ProcessFileAsync(string filePath, CancellationToken ct);
    
    bool IsFileReady(string filePath);

    int CleanGarbageFiles(string directory);
}

public class ReportFileService : IFileService<IReport>
{
    private readonly ILogger<ReportFileService> _logger;
    private readonly Settings _settings;
    
    public ReportFileService(IOptions<Settings> settings, ILogger<ReportFileService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
    }
    
    public async Task<IReport?> ProcessFileAsync(string filePath, CancellationToken ct)
    {
        var reportId = Guid.NewGuid();
        var ext = Path.GetExtension(filePath);

        var newFileName = reportId + ext;
        var archiveFilePath = Path.Combine(_settings.FullArchiveDirectory, newFileName);
        
        IReport? report = null;
        if (_settings.TextSupportedFormats.Contains(ext))
        {
            var text = await File.ReadAllTextAsync(filePath, ct);
            report = new TextContentReport(reportId, ContentType.Text, newFileName, text, DateTime.UtcNow);
        }
        else if (_settings.ImageSupportedFormats.Contains(ext))
        {
            report = new ImageContentReport(reportId, ContentType.Image, newFileName, archiveFilePath, DateTime.UtcNow);
        }

        if (ct.IsCancellationRequested)
        {
            return null;
        }
        
        if (report != null)
        {
            File.Move(filePath, archiveFilePath, true);
        }
        
        return report;
    }


    private bool IsFileSupported(string filePath)
    {
        bool isValid = false;
        
        try
        {
            if (!File.Exists(filePath))
                return false;
        
            var extension = Path.GetExtension(filePath);
            var fileInfo = new FileInfo(filePath);
        
            isValid = _settings.AllSupportedFormats.Contains(extension);
            isValid = isValid && fileInfo.Exists && fileInfo.Length != 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unexpected error on file check {filePath} : {Message}",  Path.GetFileName(filePath), ex.Message);
            return false;
        }
        
        return isValid;
    }
    
    public bool IsFileReady(string filePath)
    {
        try
        {
            bool isValid = IsFileSupported(filePath);
            if (!isValid)
                return false;
            
            using var stream =
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); // Low cost checking 
        }
        catch (IOException)
        {
            return false;
        }
        catch (Exception ex) // Non default errors 
        {
            _logger.LogWarning("Unexpected error on file check {filePath} : {Message}",  Path.GetFileName(filePath), ex.Message);
            return false;
        }
        
        return true;
    }

    public int CleanGarbageFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return 0;
        }

        var ct = 0;
        var files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            try
            {
                if (IsFileSupported(file))
                {
                    continue;
                }
                
                File.Delete(file);
                ct++;
            }
            catch (IOException)
            {
                _logger.LogDebug("Delete garbage {FileName} failed: file is in use", Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unexpected error on delete garbage {FileName}: {Message}", Path.GetFileName(file), ex.Message);
            }
        }

        return ct;
    }
}