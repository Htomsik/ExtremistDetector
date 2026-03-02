using SkiaSharp;

namespace SamplesGenerator;

public class ImageGenerator
{
    private readonly Random _rnd = new();
    
    public void Generate(string path, string text)
    {
        // Split text to chunks
        var chunks = text.Chunk(52).Select(x => new string(x)).ToList();
        int lineHeight = 40;
        int height = (chunks.Count * lineHeight) +20; // 20 is margin 
        
        // Panel 
        var info = new SKImageInfo(800, height); 
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        //  Random background color
        var bgColor = new SKColor((byte)_rnd.Next(0, 256), (byte)_rnd.Next(0, 256), (byte)_rnd.Next(0, 256));
        canvas.Clear(bgColor);

        // Random text color
        using SKPaint paint = new()
        {
            Color = new SKColor((byte)_rnd.Next(0, 256), (byte)_rnd.Next(0, 256), (byte)_rnd.Next(0, 256)),
            IsAntialias = true,
            TextSize = 24,
        };
        
        // Draw chunk
        float y = 40; 
        foreach (var chunk in chunks)
        {
            canvas.DrawText(chunk, 40, y, paint);
            y += lineHeight;
        }
        
        // Save
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }
}