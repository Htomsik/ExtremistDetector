using ExtremistDetector.Contracts.Models;

namespace Inquisitor.Services;

public interface IViolationChecker<in T>
{
    public ViolationType Check(T content); // TODO make IvioaltionCheckerInt async
}