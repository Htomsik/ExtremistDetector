namespace ExtremistDetector.Contracts.Models;

public interface IReport
{
    Guid ContentId { get; }
    
    ContentType ContentType { get; }
    
    string Source { get; }
    
    DateTime CreatedTime { get; }
}