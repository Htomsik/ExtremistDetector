namespace Inquisitor.Models;

public class OCRSettings
{
    public string DirectoryName { get; init; } 
    
    public string Language { get; init; } 
    
    public int MaxEngines { get; init; } 
    
    public int CancelTimeOutSeconds {get; init; }
    
    public string GetDirectoryPath() => Path.IsPathRooted(DirectoryName) 
        ? DirectoryName : Path.Combine(AppContext.BaseDirectory, DirectoryName);
    
    public string GetFilePath() => Path.Combine(GetDirectoryPath(), Language + ".traineddata");
}