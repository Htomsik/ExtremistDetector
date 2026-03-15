namespace Inquisitor.Models;

public class OCRSettings
{
    public string DirectoryName { get; init; } = "Tesseract";

    public string Language { get; init; } = "eng";
    
    public int MaxEngines { get; init; } 
    
    public int CancelTimeOutSeconds {get; init; }
    
    public string GetDirectoryPath() => Path.IsPathRooted(DirectoryName) 
        ? DirectoryName : Path.Combine(AppContext.BaseDirectory, DirectoryName);
    
    public string GetFilePath() => Path.Combine(GetDirectoryPath(), Language + ".traineddata");
}