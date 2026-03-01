namespace ExtremistDetector.Contracts.Models;


/// Informer to Inquisitor 
public interface IContentReport
{
    Guid ContentId { get; }
    
    string Source { get; }
    
    DateTime CreatedTime { get; }
}

public record TextContentReport(Guid ContentId,string Source, string Content, DateTime CreatedTime) : IContentReport;

public record ImageContentReport(Guid ContentId, string Source, string Url, DateTime CreatedTime) : IContentReport;

public enum ContentType
{
    Unknown,
    Text,
    Image,
}


