namespace Informer.Models;

public record Settings
{
    //Paths
    public string WorkDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "Samples");
    
    public string ArchiveDirectory => 
            _archiveDirectory ??= Path.Combine(WorkDirectory, "Processed");
    
    private string? _archiveDirectory;
    
    // Formats
    public IEnumerable<string> AllSupportedFormats => _allSupportedFormats ??= ImageSupportedFormats
                                                        .Concat(TextSupportedFormats);
    
    private IEnumerable<string>? _allSupportedFormats;
    
    public IEnumerable<string> ImageSupportedFormats { get; init; } = new[] { ".png", ".jpg", ".jpeg" };
    
    public IEnumerable<string> TextSupportedFormats { get; init; } = new[] { ".txt", ".log", ".json", ".html" };
}