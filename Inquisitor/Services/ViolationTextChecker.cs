using System.Collections.Frozen;
using System.Text.RegularExpressions;
using ExtremistDetector.Contracts.Models;
using Inquisitor.Models;

namespace Inquisitor.Services;

public class ViolationTextChecker : IViolationChecker<string>
{
    private readonly ILogger<ViolationTextChecker> _logger;
    private readonly FrozenDictionary<ViolationType, FrozenSet<string>> _violations;


    public ViolationTextChecker(ILogger<ViolationTextChecker> logger, IConfiguration configuration)
    {
        _logger = logger;
        var violationDictionary = configuration.GetSection("Violations").Get<Dictionary<string, List<string>>>();
        if (violationDictionary == null)
        {
            _logger.LogCritical("Violation Dictionary is empty");
            throw new InvalidOperationException(nameof(violationDictionary));
        }
        
        _violations = violationDictionary.Select(keyVal => 
            {
                if (Enum.TryParse<ViolationType>(keyVal.Key, out var type))
                {
                    return new
                    {
                        Type = type, 
                        Words = keyVal
                            .Value
                            .Select(x=> 
                                Regex.Replace(x, @"\s", "") 
                                .Normalize()
                                .ToLowerInvariant()
                            )
                            .ToFrozenSet(StringComparer.OrdinalIgnoreCase)
                    };
                }
                return null;
            }).Where(x => x != null)
            .ToFrozenDictionary(x => x!.Type, x => x!.Words);
    }

    public async Task<CheckerResult> Check(string content, CancellationToken cancellationToken = default)
    {
        var clearedText = Regex.Replace(content, @"\s", "")
            .Normalize()
            .ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(clearedText))
        {
            return new CheckerResult(ViolationType.None, string.Empty);
        }

        foreach (var keyVal in _violations)
        {
            var violations = keyVal.Value; 
            var violationType = keyVal.Key; 
            
            var violation = violations.FirstOrDefault(x => clearedText.Contains(x));
            if (violation != null)
            {
                return new CheckerResult(violationType, violation);
            }
        }
        
        return new CheckerResult(ViolationType.None, string.Empty);
    }
}