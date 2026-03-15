namespace Informer.Models;

public record Settings
{
    //Paths
    public string WorkDirectory { get; init; } = "Samples";
    public string ArchiveDirectory { get; init; } = "Processed";
    
    // If path is absolute - use path, else create in current app dir
    public string FullWorkDirectory => Path.IsPathRooted(WorkDirectory) 
        ? WorkDirectory 
        : Path.Combine(AppContext.BaseDirectory, WorkDirectory);

    public string FullArchiveDirectory => Path.IsPathRooted(ArchiveDirectory) 
        ? ArchiveDirectory 
        : Path.Combine(WorkDirectory, ArchiveDirectory);
    
    // Formats
    public IEnumerable<string> AllSupportedFormats => _allSupportedFormats ??= ImageSupportedFormats
                                                        .Concat(TextSupportedFormats);
    
    private IEnumerable<string>? _allSupportedFormats;
    
    public IEnumerable<string> ImageSupportedFormats { get; init; } = new[] { ".png", ".jpg", ".jpeg" };
    
    public IEnumerable<string> TextSupportedFormats { get; init; } = new[] { ".txt", ".log", ".json", ".html" };
}