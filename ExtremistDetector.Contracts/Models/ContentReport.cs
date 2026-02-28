namespace ExtremistDetector.Contracts.Models;

public record Report(string Source,
    ContentType ContentType, 
    string Content,
    DateTime CreatedTime);

public enum ContentType
{
    text,
}