using ExtremistDetector.Contracts.Models;

namespace Inquisitor.Services;

public interface IViolationChecker<in T>
{
    public  Task<ViolationType> Check(T content, CancellationToken cancellationToken = default); 
}