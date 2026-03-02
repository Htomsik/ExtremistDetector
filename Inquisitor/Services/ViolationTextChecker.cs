using System.Collections.Frozen;
using System.Text.RegularExpressions;
using ExtremistDetector.Contracts.Models;

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

    public async Task<ViolationType> Check(string content, CancellationToken cancellationToken = default)
    {
        var clearedText = Regex.Replace(content, @"\s", "")
            .Normalize()
            .ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(clearedText))
        {
            return ViolationType.None;
        }

        foreach (var keyVal in _violations)
        {
            var violation = keyVal.Value;
            var violationType = keyVal.Key;
            
            if (violation.Any(x => clearedText.Contains(x)))
            {
                return violationType;
            }
        }
        
        return ViolationType.None;
    }
}