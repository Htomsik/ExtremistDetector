using System.Collections.Concurrent;
using Inquisitor.Models;
using Microsoft.Extensions.Options;
using Tesseract;

namespace Inquisitor.Infrastructure;

public class TesseractOCRProvider : IOCRProvider, IDisposable
{
    private readonly ConcurrentBag<TesseractEngine> _engines = new ConcurrentBag<TesseractEngine>();
    private readonly SemaphoreSlim _semaphore; 
    
    private readonly OCRSettings _ocrSettings;
    private readonly ILogger<TesseractOCRProvider> _logger;

    public TesseractOCRProvider(ILogger<TesseractOCRProvider> logger,
        IOptions<OCRSettings> ocrSettings)
    {
        _ocrSettings =  ocrSettings.Value;
        _logger = logger;
        
        _semaphore = new SemaphoreSlim(_ocrSettings.MaxEngines, _ocrSettings.MaxEngines);

        for (int i = 0; i < _ocrSettings.MaxEngines; i++)
        {
            var engine  = new TesseractEngine(_ocrSettings.GetDirectoryPath(), _ocrSettings.Language, EngineMode.Default);
            
            _engines.Add(engine);
        }     
    }
    
    public async  Task<string> GetTextFromUrl(string url, CancellationToken cancellationToken = default)
    {
        var timeOut = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeOut.CancelAfter(TimeSpan.FromSeconds(_ocrSettings.CancelTimeOutSeconds));
        
        try
        {
            await _semaphore.WaitAsync(timeOut.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Timeout for OCR engine: {Url}", url);
            return string.Empty;
        }
        
        // It's allow reuse multiple OCRs
        if (_engines.TryTake(out var engine))
        {
            try
            {
                using var img = Pix.LoadFromFile(url);
                using var page = engine.Process(img);
                var text = page.GetText();
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR failed for {Url}", url);
                return string.Empty;
            }
            finally
            {
                _semaphore.Release(); 
                _engines.Add(engine);
            }
        }
        
        return string.Empty;
    }

    public void Dispose()
    {
        while (_engines.TryTake(out var engine)) 
            engine.Dispose();
        
        _semaphore.Dispose();
    }
}