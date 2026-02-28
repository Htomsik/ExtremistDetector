namespace ExtremistDetector.Contracts.Models;


/// Informer to Inquisitor 
public interface IContentReport
{
    string Source { get; }
    
    DateTime CreatedTime { get; }
}

public record TextContentReport(string Source, string Content, DateTime CreatedTime) : IContentReport;

public record ImageContentReport(string Source, string Url, DateTime CreatedTime) : IContentReport;

public enum ContentType
{
    Unknown,
    Text,
    Image,
}


