using System.Text.Json;
using ExtremistDetector.Contracts.Models;
using StackExchange.Redis;

namespace Butcher.Services;

public class ViolationService : IViolationService
{
    private readonly IDatabase _redis;
    
    public ViolationService(ILogger<ViolationService> logger, IConnectionMultiplexer redis) 
    {
        _redis = redis.GetDatabase();
    }
    
    public async Task Resolve(ViolationReport violationReport)
    {
        var jsonReport = JsonSerializer.Serialize(violationReport);
        
        // Day history
        await _redis.ListLeftPushAsync("Violations:Day", jsonReport); 
        await _redis.KeyExpireAsync("Violations:Day", TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);
        
        // Last 10 history
        await _redis.ListLeftPushAsync("Violations:Latest", jsonReport);
        await _redis.ListTrimAsync("Violations:Latest", 0, 9);
        
        // Global analytic
        await _redis.HashIncrementAsync("Analytic:Count", violationReport.ViolationType.ToString());
        
        // Days analytic
        await _redis.HashIncrementAsync("Analytic:Count-Day", violationReport.ViolationType.ToString());
        await _redis.KeyExpireAsync("Analytic:Count-Day", TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);
    }
}