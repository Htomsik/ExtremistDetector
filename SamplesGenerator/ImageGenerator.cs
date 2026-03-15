using SkiaSharp;

namespace SamplesGenerator;

public class ImageGenerator
{
    private readonly Random _random = new();
    
    private readonly string[] _fonts = { "Arial", "Times New Roman", "Verdana", "Courier New" };
    
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
        var bgColor = new SKColor((byte)_random.Next(0, 256), (byte)_random.Next(0, 256), (byte)_random.Next(0, 256));
        canvas.Clear(bgColor);

        // Randomize text
        var font = _fonts[_random.Next(_fonts.Length)];
        
        var fontStyle = _random.Next(3) switch {
            1 => SKFontStyle.Bold,
            2 => SKFontStyle.Italic,
            _ => SKFontStyle.Normal
        };
        
        using SKPaint paint = new()
        {
            Color = new SKColor((byte)_random.Next(0, 256), (byte)_random.Next(0, 256), (byte)_random.Next(0, 256)),
            IsAntialias = true,
            TextSize = 24,
            Typeface = SKTypeface.FromFamilyName(font, fontStyle)
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