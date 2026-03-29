using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryStatisticsService : IStatisticsService
{
    private readonly IWorkoutHistoryService _historyService;
    private readonly IRoutineService _routineService;

    public InMemoryStatisticsService(
        IWorkoutHistoryService historyService,
        IRoutineService routineService)
    {
        _historyService = historyService;
        _routineService = routineService;
    }

    public async Task<IReadOnlyList<int>> GetAvailableTimelineYearsAsync(Guid routineId, Guid? workoutId)
    {
        if (routineId == Guid.Empty)
            return Array.Empty<int>();

        var sessions = (await _historyService.GetAllAsync())
            .Where(x => x.RoutineId == routineId)
            .Where(x => !workoutId.HasValue || x.WorkoutId == workoutId.Value)
            .Select(x => x.PerformedAt.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        return sessions;
    }

    public async Task<StatisticsSnapshot> GetSnapshotAsync(
        Guid routineId,
        Guid? workoutId,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        if (routineId == Guid.Empty)
            return BuildEmptySnapshot();

        var routines = await _routineService.GetAllAsync();
        var routine = routines.FirstOrDefault(x => x.Id == routineId);
        if (routine is null)
            return BuildEmptySnapshot();

        var selectedWorkouts = workoutId.HasValue
            ? routine.Workouts.Where(x => x.Id == workoutId.Value).ToList()
            : routine.Workouts.ToList();

        if (selectedWorkouts.Count == 0)
            return BuildEmptySnapshot();

        var currentExercises = selectedWorkouts
            .SelectMany(x => x.Exercises)
            .ToList();

        var allSessions = (await _historyService.GetAllAsync())
            .Where(x => x.RoutineId == routineId)
            .Where(x => !workoutId.HasValue || x.WorkoutId == workoutId.Value)
            .OrderBy(x => x.PerformedAt)
            .ToList();

        var timelineSessions = FilterTimelineSessions(allSessions, bucket, year, quarter);

        var selectionTitle = workoutId.HasValue
            ? selectedWorkouts.First().Name
            : routine.Name;

        return new StatisticsSnapshot
        {
            Title = selectionTitle,
            HasSelection = true,
            HasWorkoutSelection = workoutId.HasValue,
            SelectionSummary = workoutId.HasValue
                ? $"Preferences uses the current workout setup for '{selectedWorkouts.First().Name}'. Timeline uses logged history for {year}, {quarter}."
                : $"Preferences uses the current routine setup for '{routine.Name}'. Timeline uses logged history for {year}, {quarter}.",

            CurrentWorkoutCount = selectedWorkouts.Count,
            CurrentExerciseCount = currentExercises.Count,
            CurrentTotalSets = currentExercises.Sum(x => x.Sets.Count),
            CurrentPlannedVolume = Math.Round(currentExercises.Sum(GetWorkoutExerciseVolume), 1),

            MuscleGroupPreference = Normalize(BuildCurrentSetsPerMuscleGroup(currentExercises)),
            EquipmentPreference = Normalize(BuildCurrentBreakdown(currentExercises, x => x.Equipment)),
            MechanicPreference = Normalize(BuildCurrentBreakdown(currentExercises, x => x.Mechanic)),
            ExerciseVolumePreference = Normalize(BuildCurrentExerciseVolumeBreakdown(currentExercises)),

            TimelineWorkoutCount = timelineSessions.Count,
            TimelineTotalSets = timelineSessions.Sum(x => x.TotalSets),
            TimelineTotalVolume = Math.Round(timelineSessions.Sum(x => x.TotalVolume), 1),
            TimelineEstimatedCaloriesBurned = Math.Round(timelineSessions.Sum(x => x.EstimatedCaloriesBurned), 0),

            VolumeTrend = BuildVolumeTrend(timelineSessions, bucket, year, quarter),
            LiftedWeightTrend = BuildLiftedWeightTrend(timelineSessions, bucket, year, quarter),
            GymVisitFrequencyTrend = BuildGymVisitFrequencyTrend(timelineSessions, bucket, year, quarter),
            ExerciseVolumeTrendSeries = BuildExerciseVolumeTrendSeries(timelineSessions, bucket, year, quarter),
            GymVisitFrequencyTitle = bucket switch
            {
                StatisticsTimelineBucket.Days => "Gym Visits Per Day",
                StatisticsTimelineBucket.Weeks => "Gym Visits Per Week",
                StatisticsTimelineBucket.Months => "Gym Visits Per Month",
                _ => "Gym Visits"
            },
            Plateau = BuildPlateau(allSessions)
        };
    }

    private static StatisticsSnapshot BuildEmptySnapshot()
    {
        return new StatisticsSnapshot
        {
            HasSelection = false,
            SelectionSummary = "Select a routine to view current setup preferences and logged timeline history."
        };
    }

    private static List<LoggedWorkoutSession> FilterTimelineSessions(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        return bucket switch
        {
            StatisticsTimelineBucket.Months => sessions
                .Where(x => x.PerformedAt.Year == year)
                .OrderBy(x => x.PerformedAt)
                .ToList(),

            StatisticsTimelineBucket.Days or StatisticsTimelineBucket.Weeks => sessions
                .Where(x => x.PerformedAt.Year == year)
                .Where(x => GetQuarter(x.PerformedAt.Month) == quarter)
                .OrderBy(x => x.PerformedAt)
                .ToList(),

            _ => sessions.OrderBy(x => x.PerformedAt).ToList()
        };
    }

    private static List<TrendPoint> BuildVolumeTrend(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        var dates = BuildBucketDates(bucket, year, quarter);

        var grouped = sessions
            .GroupBy(x => NormalizeTimelineBucket(bucket, x.PerformedAt.Date))
            .ToDictionary(
                x => x.Key,
                x => Math.Round(x.Sum(y => y.TotalVolume), 1));

        return dates
            .Select(date => new TrendPoint
            {
                Date = date,
                Label = FormatTimelineBucketLabel(bucket, date),
                Value = grouped.GetValueOrDefault(date)
            })
            .ToList();
    }

    private static List<TrendPoint> BuildLiftedWeightTrend(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        var dates = BuildBucketDates(bucket, year, quarter);

        var grouped = sessions
            .GroupBy(x => NormalizeTimelineBucket(bucket, x.PerformedAt.Date))
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var weights = group
                        .SelectMany(s => s.Exercises)
                        .SelectMany(e => e.Sets)
                        .Where(set => set.WeightKg.HasValue && set.WeightKg.Value > 0)
                        .Select(set => set.WeightKg!.Value)
                        .ToList();

                    return weights.Count == 0 ? 0 : Math.Round(weights.Average(), 1);
                });

        return dates
            .Select(date => new TrendPoint
            {
                Date = date,
                Label = FormatTimelineBucketLabel(bucket, date),
                Value = grouped.GetValueOrDefault(date)
            })
            .ToList();
    }

    private static List<TrendPoint> BuildGymVisitFrequencyTrend(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        var dates = BuildBucketDates(bucket, year, quarter);

        var grouped = sessions
            .GroupBy(x => NormalizeTimelineBucket(bucket, x.PerformedAt.Date))
            .ToDictionary(x => x.Key, x => (double)x.Count());

        return dates
            .Select(date => new TrendPoint
            {
                Date = date,
                Label = FormatTimelineBucketLabel(bucket, date),
                Value = grouped.GetValueOrDefault(date)
            })
            .ToList();
    }

    private static List<TrendSeries> BuildExerciseVolumeTrendSeries(
        IReadOnlyList<LoggedWorkoutSession> sessions,
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        var dates = BuildBucketDates(bucket, year, quarter);

        var topExerciseNames = sessions
            .SelectMany(x => x.Exercises)
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new
            {
                Name = x.Key,
                Volume = x.Sum(e => e.TotalVolume)
            })
            .OrderByDescending(x => x.Volume)
            .Take(6)
            .Select(x => x.Name)
            .ToList();

        var series = new List<TrendSeries>();

        foreach (var exerciseName in topExerciseNames)
        {
            var grouped = sessions
                .SelectMany(
                    session => session.Exercises
                        .Where(ex => string.Equals(ex.Name, exerciseName, StringComparison.OrdinalIgnoreCase))
                        .Select(ex => new
                        {
                            Bucket = NormalizeTimelineBucket(bucket, session.PerformedAt.Date),
                            ex.TotalVolume
                        }))
                .GroupBy(x => x.Bucket)
                .ToDictionary(x => x.Key, x => Math.Round(x.Sum(y => y.TotalVolume), 1));

            series.Add(new TrendSeries
            {
                Label = exerciseName,
                Points = dates.Select(date => new TrendPoint
                {
                    Date = date,
                    Label = FormatTimelineBucketLabel(bucket, date),
                    Value = grouped.GetValueOrDefault(date)
                }).ToList()
            });
        }

        return series;
    }

    private static DateTime NormalizeTimelineBucket(StatisticsTimelineBucket bucket, DateTime date)
    {
        return bucket switch
        {
            StatisticsTimelineBucket.Days => date.Date,
            StatisticsTimelineBucket.Weeks => NormalizeWeekOfMonth(date),
            StatisticsTimelineBucket.Months => new DateTime(date.Year, date.Month, 1),
            _ => date.Date
        };
    }

    private static DateTime NormalizeWeekOfMonth(DateTime date)
    {
        var startDay = 1 + (((date.Day - 1) / 7) * 7);
        return new DateTime(date.Year, date.Month, startDay);
    }

    private static List<DateTime> BuildBucketDates(
        StatisticsTimelineBucket bucket,
        int year,
        StatisticsQuarter quarter)
    {
        var quarterMonths = GetQuarterMonths(quarter);

        return bucket switch
        {
            StatisticsTimelineBucket.Days => quarterMonths
                .SelectMany(month =>
                {
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    return Enumerable.Range(1, daysInMonth)
                        .Select(day => new DateTime(year, month, day));
                })
                .ToList(),

            StatisticsTimelineBucket.Weeks => quarterMonths
                .SelectMany(month =>
                {
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    var weekCount = (int)Math.Ceiling(daysInMonth / 7.0);

                    return Enumerable.Range(0, weekCount)
                        .Select(index => new DateTime(year, month, 1 + (index * 7)));
                })
                .ToList(),

            StatisticsTimelineBucket.Months => Enumerable.Range(1, 12)
                .Select(month => new DateTime(year, month, 1))
                .ToList(),

            _ => new List<DateTime>()
        };
    }

    private static string FormatTimelineBucketLabel(StatisticsTimelineBucket bucket, DateTime date)
    {
        return bucket switch
        {
            StatisticsTimelineBucket.Days => date.ToString("dd MMM"),
            StatisticsTimelineBucket.Weeks => $"W{(((date.Day - 1) / 7) + 1)}",
            StatisticsTimelineBucket.Months => date.ToString("MMM"),
            _ => date.ToString("dd MMM")
        };
    }

    private static StatisticsQuarter GetQuarter(int month)
    {
        return month switch
        {
            <= 3 => StatisticsQuarter.Q1,
            <= 6 => StatisticsQuarter.Q2,
            <= 9 => StatisticsQuarter.Q3,
            _ => StatisticsQuarter.Q4
        };
    }

    private static int[] GetQuarterMonths(StatisticsQuarter quarter)
    {
        return quarter switch
        {
            StatisticsQuarter.Q1 => new[] { 1, 2, 3 },
            StatisticsQuarter.Q2 => new[] { 4, 5, 6 },
            StatisticsQuarter.Q3 => new[] { 7, 8, 9 },
            StatisticsQuarter.Q4 => new[] { 10, 11, 12 },
            _ => new[] { 1, 2, 3 }
        };
    }

    private static List<BreakdownItem> BuildCurrentSetsPerMuscleGroup(IReadOnlyList<WorkoutExercise> exercises)
    {
        var totals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in exercises)
        {
            var setCount = exercise.Sets.Count;

            foreach (var category in exercise.PrimaryMuscleCategories.Where(x => !string.IsNullOrWhiteSpace(x)))
                totals[category] = totals.GetValueOrDefault(category) + setCount;

            foreach (var category in exercise.SecondaryMuscleCategories.Where(x => !string.IsNullOrWhiteSpace(x)))
                totals[category] = totals.GetValueOrDefault(category) + (setCount * 0.5);
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

    private static List<BreakdownItem> BuildCurrentBreakdown(
        IReadOnlyList<WorkoutExercise> exercises,
        Func<WorkoutExercise, string> selector)
    {
        return exercises
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

    private static List<BreakdownItem> BuildCurrentExerciseVolumeBreakdown(
        IReadOnlyList<WorkoutExercise> exercises)
    {
        return exercises
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new BreakdownItem
            {
                Label = x.Key,
                Value = Math.Round(x.Sum(GetWorkoutExerciseVolume), 1)
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    private static double GetWorkoutExerciseVolume(WorkoutExercise exercise)
    {
        return exercise.Sets.Sum(set =>
        {
            var reps = set.Reps ?? 0;
            var weight = set.WeightKg ?? 0;
            return reps > 0 && weight > 0 ? reps * weight : 0;
        });
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

        var last3 = ordered.TakeLast(3).Average(x => x.TotalVolume);
        var previous3 = ordered.Skip(Math.Max(0, ordered.Count - 6)).Take(3).Average(x => x.TotalVolume);

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

    private static List<BreakdownItem> Normalize(List<BreakdownItem> items)
    {
        if (items.Count == 0)
            return items;

        var total = items.Sum(x => x.Value);
        if (total <= 0)
            return items;

        foreach (var item in items)
            item.Percent = item.Value / total;

        return items;
    }
}