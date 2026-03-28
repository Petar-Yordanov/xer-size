using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IStatisticsService
{
    Task<IReadOnlyList<int>> GetAvailableTimelineYearsAsync(Guid routineId, Guid? workoutId);

    Task<StatisticsSnapshot> GetSnapshotAsync(
        Guid routineId,
        Guid? workoutId,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter);
}