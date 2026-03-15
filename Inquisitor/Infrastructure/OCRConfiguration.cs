using ExtremistDetector.Contracts.Constants;
using Inquisitor.Models;
using Microsoft.Extensions.Options;

public static class OCRConfiguration
{
    public static async Task EnsureTesseract(this IServiceProvider services)
    {
        var ocrSettings = services.GetRequiredService<IOptions<OCRSettings>>().Value;
        var logger = services.GetRequiredService<ILogger<string>>();
        
        if (File.Exists(ocrSettings.GetFilePath()))
        {
            logger.LogInformation("Tesseract data founded: {Path}", ocrSettings.DirectoryName);
            return;
        }

        logger.LogWarning("Tesseract data doesn't found. Downloading in: {Path}", ocrSettings.GetFilePath());
        
        try
        {
            if (!Directory.Exists(ocrSettings.GetDirectoryPath())) 
                Directory.CreateDirectory(ocrSettings.GetDirectoryPath());
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# App / ExtremistDetector");
            var data = await client.GetByteArrayAsync(OCRConstants.TesseractEngDataUrl);
            
            await File.WriteAllBytesAsync(ocrSettings.GetFilePath(), data);

            logger.LogInformation("Tesseract data downloaded");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed download Tesseract data");
            throw; 
        }
    }
}