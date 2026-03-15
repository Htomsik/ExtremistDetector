using ExtremistDetector.Contracts.Models;

namespace Inquisitor.Models;

public record CheckerResult(ViolationType Type, string TriggeredData);