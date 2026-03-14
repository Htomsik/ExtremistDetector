using System.Text.Json;
using ExtremistDetector.Contracts.Constants;
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
    
    public async Task Resolve(ViolationReport violationReport, CancellationToken cancellationToken = default)
    {
        var jsonReport = JsonSerializer.Serialize(violationReport);
        var contentType = violationReport.ViolationType.ToString();
        var contentId = violationReport.ContentId.ToString();
        var leadTime = violationReport.LeadTimeSec();
        var processTime = violationReport.ProcessTimeSec();

        // TODO move all this trash to repository
        var batch = _redis.CreateBatch();
        
        // History
        _= batch.ListLeftPushAsync(RedisConstants.DayHistory, jsonReport);  
        _= batch.KeyExpireAsync(RedisConstants.DayHistory, TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);
        
        _= batch.ListLeftPushAsync(RedisConstants.LatestHistory, jsonReport);
        _= batch.ListTrimAsync(RedisConstants.LatestHistory, 0, 9);
        
        // analytic
        _= batch.HashIncrementAsync(RedisConstants.TypeTotal, contentType);
        
        _= batch.HashIncrementAsync(RedisConstants.TypeDay, contentType);
        _= batch.KeyExpireAsync(RedisConstants.TypeDay, TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);

        if (!string.IsNullOrWhiteSpace(violationReport.TriggeredData))
        {
            _= batch.HashIncrementAsync(RedisConstants.TriggeredTotal, violationReport.TriggeredData);
            
            _= batch.HashIncrementAsync(RedisConstants.TriggeredDay, violationReport.TriggeredData);
            _= batch.KeyExpireAsync(RedisConstants.TriggeredDay, TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);
        }
        
        // performance
        _ = batch.SortedSetAddAsync(RedisConstants.LatestLeadSlowest, contentId, leadTime);
        _ = batch.SortedSetRemoveRangeByRankAsync(RedisConstants.LatestLeadSlowest, 0, -11); // top 10
        _= batch.KeyExpireAsync(RedisConstants.LatestLeadSlowest, TimeSpan.FromHours(1), ExpireWhen.HasNoExpiry);
        
        _ = batch.SortedSetAddAsync(RedisConstants.DayLeadSlowest, contentId, leadTime);
        _ = batch.SortedSetRemoveRangeByRankAsync(RedisConstants.DayLeadSlowest, 0, -11);  // top 10
        _= batch.KeyExpireAsync(RedisConstants.DayLeadSlowest, TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);
        
        _ = batch.SortedSetAddAsync(RedisConstants.LatestProcessSlowest, contentId, processTime);
        _ = batch.SortedSetRemoveRangeByRankAsync(RedisConstants.LatestProcessSlowest, 0, -11); // top 10
        _= batch.KeyExpireAsync(RedisConstants.LatestProcessSlowest, TimeSpan.FromHours(1), ExpireWhen.HasNoExpiry);
        
        _ = batch.SortedSetAddAsync(RedisConstants.DayProcessSlowest, contentId, processTime);
        _ = batch.SortedSetRemoveRangeByRankAsync(RedisConstants.DayProcessSlowest, 0, -11);  // top 10
        _= batch.KeyExpireAsync(RedisConstants.DayProcessSlowest, TimeSpan.FromDays(1), ExpireWhen.HasNoExpiry);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        await Task.Run(() => batch.Execute(), cancellationToken); // redis is mono,
    }
}