namespace ExtremistDetector.Contracts.Models;

/// Inquisitor to Butcher

public interface IViolationReport : IReport
{
    ViolationType ViolationType { get; }
    
    string TriggeredData { get; }
    
    DateTime StartProcessTime { get; }
    
    DateTime EndProcessTime { get; }
}


public record ViolationReport(
    Guid ContentId,
    ContentType ContentType,
    ViolationType ViolationType, 
    string Source, 
    string Content, 
    string TriggeredData,
    DateTime CreatedTime,
    DateTime StartProcessTime,
    DateTime EndProcessTime
    ) : IViolationReport
{
    /// <summary>
    ///    All time in queue + process
    /// </summary>
    public double LeadTimeSec() => (EndProcessTime - CreatedTime).TotalSeconds; 
    
    /// <summary>
    ///     Only process time, exclude queue time
    /// </summary>
    public double ProcessTimeSec() => (EndProcessTime - StartProcessTime).TotalSeconds; 
}


public enum ViolationType
{
    None,
    ToDelete,
    Penalty,
    CreatorDestroy
}

