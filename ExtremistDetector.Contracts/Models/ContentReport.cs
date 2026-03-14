namespace ExtremistDetector.Contracts.Models;

/// Informer to Inquisitor 
// MassTransit Use types for routing

public record TextContentReport(
    Guid ContentId,
    ContentType ContentType,
    string Source,
    string Content,
    DateTime CreatedTime) : IReport;

public record ImageContentReport(
    Guid ContentId, 
    ContentType ContentType, 
    string Source, 
    string Url, 
    DateTime CreatedTime) : IReport;