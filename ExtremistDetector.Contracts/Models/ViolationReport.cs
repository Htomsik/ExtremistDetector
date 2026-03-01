namespace ExtremistDetector.Contracts.Models;

/// Inquisitor to Butcher 
public record ViolationReport(ViolationType ViolationType, 
    string Source, 
    string Content, 
    DateTime CreatedTime);


public enum ViolationType
{
    None,
    ToDelete,
    Penalty,
    CreatorDestroy
}