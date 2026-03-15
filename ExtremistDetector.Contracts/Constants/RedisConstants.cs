namespace ExtremistDetector.Contracts.Constants;

public class RedisConstants
{
    public const string DayHistory = "violations:day";
    public const string LatestHistory = "violations:latest";

    public const string TypeTotal = "analytic:type:total";
    public const string TypeDay = "analytic:type:day";

    public const string TriggeredTotal = "analytic:triggered:total";
    public const string TriggeredDay = "analytic:triggered:day";

    public const string LatestLeadSlowest = "performance:lead:slowest:latest";
    public const string DayLeadSlowest = "performance:lead:slowest:day";

    public const string LatestProcessSlowest = "performance:process:slowest:latest";
    public const string DayProcessSlowest = "performance:process:slowest:day";
}