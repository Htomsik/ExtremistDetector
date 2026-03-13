using ExtremistDetector.Contracts.Models;

namespace Butcher.Services;

public interface IViolationService
{
    Task Resolve(ViolationReport violationReport);
}