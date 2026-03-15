using ExtremistDetector.Contracts.Models;
using Inquisitor.Models;

namespace Inquisitor.Services;

public interface IViolationChecker<in T>
{
    public  Task<CheckerResult> Check(T content, CancellationToken cancellationToken = default); 
}