using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsSnapshot> GetSnapshotAsync(
        StatisticsRange range,
        StatisticsScopeKind scopeKind,
        Guid routineId,
        Guid? workoutId);
}