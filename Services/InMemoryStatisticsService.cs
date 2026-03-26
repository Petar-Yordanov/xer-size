using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryStatisticsService : IStatisticsService
{
    private readonly IWorkoutHistoryService _historyService;

    public InMemoryStatisticsService(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task<StatisticsSnapshot> GetSnapshotAsync(
        StatisticsRange range,
        StatisticsScopeKind scopeKind,
        Guid routineId,
        Guid? workoutId)
    {
        var allSessions = (await _historyService.GetAllAsync())
            .OrderBy(x => x.PerformedAt)
            .ToList();

        var scopedAllTime = allSessions
            .Where(x => x.RoutineId == routineId)
            .Where(x => scopeKind == StatisticsScopeKind.Routine || !workoutId.HasValue || x.WorkoutId == workoutId.Value)
            .OrderBy(x => x.PerformedAt)
            .ToList();

        var cutoff = GetCutoff(range);

        var filtered = scopedAllTime
            .Where(x => x.PerformedAt.Date >= cutoff)
            .OrderBy(x => x.PerformedAt)
            .ToList();

        var totalDaysInRange = Math.Max(1, (DateTime.Today - cutoff).Days + 1);
        var workoutDays = filtered
            .Select(x => x.PerformedAt.Date)
            .Distinct()
            .Count();

        var title = scopeKind == StatisticsScopeKind.Workout
            ? scopedAllTime.FirstOrDefault()?.WorkoutName ?? "Workout Statistics"
            : scopedAllTime.FirstOrDefault()?.RoutineName ?? "Routine Statistics";

        var snapshot = new StatisticsSnapshot
        {
            Title = title,
            WorkoutCount = filtered.Count,
            RestDays = Math.Max(0, totalDaysInRange - workoutDays),
            MuscleGroupsHitCount = filtered
                .SelectMany(x => x.Exercises)
                .SelectMany(x => x.PrimaryMuscleCategories.Concat(x.SecondaryMuscleCategories))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            TotalSets = filtered.Sum(x => x.TotalSets),
            TotalVolume = Math.Round(filtered.Sum(x => x.TotalVolume), 1),
            EstimatedCaloriesBurned = Math.Round(filtered.Sum(x => x.EstimatedCaloriesBurned), 0),

            VolumeTrend = BuildTrend(filtered, range, x => x.TotalVolume),
            WeightTrend = BuildTrend(filtered, range, x => x.BodyWeightKg),

            SetsPerMuscleGroup = Normalize(BuildSetsPerMuscleGroup(filtered)),
            EquipmentPreference = Normalize(BuildBreakdown(filtered, x => x.Equipment)),
            MechanicPreference = Normalize(BuildBreakdown(filtered, x => x.Mechanic)),
            LateralityPreference = Normalize(BuildBreakdown(filtered, x => x.LimbInvolvement)),
            NeglectedMuscles = Normalize(BuildNeglectedMuscles(scopedAllTime, filtered)),

            Plateau = BuildPlateau(scopedAllTime),
            Calendar = BuildCalendar(filtered, cutoff)
        };

        return snapshot;
    }

    private static DateTime GetCutoff(StatisticsRange range)
    {
        return range switch
        {
            StatisticsRange.LastWeek => DateTime.Today.AddDays(-6),
            StatisticsRange.Last2Weeks => DateTime.Today.AddDays(-13),
            StatisticsRange.LastMonth => DateTime.Today.AddMonths(-1).AddDays(1),
            StatisticsRange.Last3Months => DateTime.Today.AddMonths(-3).AddDays(1),
            StatisticsRange.Last6Months => DateTime.Today.AddMonths(-6).AddDays(1),
            StatisticsRange.LastYear => DateTime.Today.AddYears(-1).AddDays(1),
            _ => DateTime.Today.AddMonths(-1).AddDays(1)
        };
    }

    private static List<TrendPoint> BuildTrend(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsRange range,
        Func<LoggedWorkoutSession, double> selector)
    {
        return sessions
            .GroupBy(x => NormalizeBucket(range, x.PerformedAt.Date))
            .OrderBy(x => x.Key)
            .Select(x => new TrendPoint
            {
                Date = x.Key,
                Value = Math.Round(x.Average(selector), 1)
            })
            .ToList();
    }

    private static DateTime NormalizeBucket(StatisticsRange range, DateTime date)
    {
        if (range is StatisticsRange.LastWeek or StatisticsRange.Last2Weeks)
            return date;

        // week bucket starting Monday
        int diff = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-diff);
    }

    private static List<BreakdownItem> BuildSetsPerMuscleGroup(IReadOnlyList<LoggedWorkoutSession> sessions)
    {
        var totals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in sessions.SelectMany(x => x.Exercises))
        {
            var setCount = exercise.Sets.Count;

            foreach (var category in exercise.PrimaryMuscleCategories.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                totals[category] = totals.GetValueOrDefault(category) + setCount;
            }

            foreach (var category in exercise.SecondaryMuscleCategories.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                totals[category] = totals.GetValueOrDefault(category) + (setCount * 0.5);
            }
        }

        return totals
            .OrderByDescending(x => x.Value)
            .Select(x => new BreakdownItem
            {
                Label = x.Key,
                Value = Math.Round(x.Value, 1)
            })
            .ToList();
    }

    private static List<BreakdownItem> BuildBreakdown(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        Func<LoggedWorkoutExercise, string> selector)
    {
        return sessions
            .SelectMany(x => x.Exercises)
            .Where(x => !string.IsNullOrWhiteSpace(selector(x)))
            .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
            .Select(x => new BreakdownItem
            {
                Label = x.Key,
                Value = x.Sum(e => e.Sets.Count)
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    private static List<BreakdownItem> BuildNeglectedMuscles(
        IReadOnlyList<LoggedWorkoutSession> scopedAllTime,
        IReadOnlyList<LoggedWorkoutSession> filtered)
    {
        var knownCategories = scopedAllTime
            .SelectMany(x => x.Exercises)
            .SelectMany(x => x.PrimaryMuscleCategories.Concat(x.SecondaryMuscleCategories))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentLoad = BuildSetsPerMuscleGroup(filtered)
            .ToDictionary(x => x.Label, x => x.Value, StringComparer.OrdinalIgnoreCase);

        return knownCategories
            .Select(x => new BreakdownItem
            {
                Label = x,
                Value = currentLoad.GetValueOrDefault(x)
            })
            .OrderBy(x => x.Value)
            .Take(6)
            .ToList();
    }

    private static PlateauInsight BuildPlateau(IReadOnlyList<LoggedWorkoutSession> sessions)
    {
        if (sessions.Count < 6)
        {
            return new PlateauInsight
            {
                IsPlateaued = false,
                Summary = "Not enough data yet to detect a plateau."
            };
        }

        var ordered = sessions.OrderBy(x => x.PerformedAt).ToList();

        var last3 = ordered
            .TakeLast(3)
            .Average(x => x.TotalVolume);

        var previous3 = ordered
            .Skip(Math.Max(0, ordered.Count - 6))
            .Take(3)
            .Average(x => x.TotalVolume);

        if (previous3 <= 0)
        {
            return new PlateauInsight
            {
                IsPlateaued = false,
                Summary = "Not enough comparable volume data yet."
            };
        }

        var deltaPercent = ((last3 - previous3) / previous3) * 100.0;

        if (Math.Abs(deltaPercent) <= 3.0)
        {
            return new PlateauInsight
            {
                IsPlateaued = true,
                Summary = $"Volume is nearly flat ({deltaPercent:F1}% vs previous block). Possible plateau."
            };
        }

        return new PlateauInsight
        {
            IsPlateaued = false,
            Summary = deltaPercent > 0
                ? $"Volume is trending up ({deltaPercent:F1}% vs previous block)."
                : $"Volume is trending down ({deltaPercent:F1}% vs previous block)."
        };
    }

    private static List<CalendarDayItem> BuildCalendar(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        DateTime cutoff)
    {
        var volumeByDay = sessions
            .GroupBy(x => x.PerformedAt.Date)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.TotalVolume));

        var items = new List<CalendarDayItem>();

        for (var day = cutoff.Date; day <= DateTime.Today; day = day.AddDays(1))
        {
            var workedOut = volumeByDay.TryGetValue(day, out var volume);

            items.Add(new CalendarDayItem
            {
                Date = day,
                WorkedOut = workedOut,
                Volume = workedOut ? volume : 0
            });
        }

        return items;
    }

    private static List<BreakdownItem> Normalize(List<BreakdownItem> items)
    {
        if (items.Count == 0)
            return items;

        var max = items.Max(x => x.Value);

        foreach (var item in items)
        {
            item.Percent = max <= 0 ? 0 : item.Value / max;
        }

        return items;
    }
}