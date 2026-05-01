using System.Globalization;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;

namespace XerSize.Services;

public sealed class DashboardStatisticsService
{
    private readonly WorkoutHistoryService workoutHistoryService;
    private readonly UserSettingsService userSettingsService;

    public DashboardStatisticsService(
        WorkoutHistoryService workoutHistoryService,
        UserSettingsService userSettingsService)
    {
        this.workoutHistoryService = workoutHistoryService;
        this.userSettingsService = userSettingsService;
    }

    public DashboardMetrics GetDashboardMetrics(DateTime from, DateTime to, string rangeId)
    {
        var history = workoutHistoryService.GetHistory(from, to);
        var completedWorkoutCount = history.Count;
        var estimatedCaloriesByWorkout = CalculateEstimatedCaloriesByWorkout(history);
        var totalEstimatedCalories = estimatedCaloriesByWorkout.Sum();

        if (completedWorkoutCount == 0)
        {
            return new DashboardMetrics
            {
                AverageEstimatedCaloriesBurned = 0,
                TotalEstimatedCaloriesBurned = 0,
                AverageWorkoutMinutes = 0,
                AverageSessionsPerWeek = 0,
                TargetProgress = BuildTargetProgress(rangeId, 0, 0),
                ActivityTimeline = BuildActivityTimeline([], from, to),
                PreferredMuscleGroups = [],
                PreferredEquipment = [],
                PreferredMechanics = []
            };
        }

        var historyExercises = history
            .Where(workout => !workout.ExcludeMetadataFromMetrics)
            .SelectMany(workout => workoutHistoryService.GetExercises(workout.Id))
            .ToList();

        var averageCalories = estimatedCaloriesByWorkout.Count == 0
            ? 0
            : estimatedCaloriesByWorkout.Average();

        var averageWorkoutMinutes = history.Average(workout => Math.Max(0, workout.DurationMinutes));
        var averageSessionsPerWeek = CalculateSessionsPerWeek(completedWorkoutCount, from, to);

        return new DashboardMetrics
        {
            AverageEstimatedCaloriesBurned = averageCalories,
            TotalEstimatedCaloriesBurned = totalEstimatedCalories,
            AverageWorkoutMinutes = averageWorkoutMinutes,
            AverageSessionsPerWeek = averageSessionsPerWeek,
            TargetProgress = BuildTargetProgress(rangeId, completedWorkoutCount, totalEstimatedCalories),
            ActivityTimeline = BuildActivityTimeline(history, from, to),
            PreferredMuscleGroups = BuildPreferredMuscleGroups(historyExercises),
            PreferredEquipment = BuildPreferredEquipment(historyExercises),
            PreferredMechanics = BuildPreferredMechanics(historyExercises)
        };
    }

    private List<double> CalculateEstimatedCaloriesByWorkout(IReadOnlyList<HistoryWorkoutItemModel> history)
    {
        if (!TryGetUserProfileForCalories(
            out var weightKg,
            out var heightCm,
            out var age,
            out var sex))
        {
            return [];
        }

        var bmr = CalculateBmrMifflin(weightKg, heightCm, age, sex);

        return history
            .Where(workout => !workout.ExcludeCaloriesFromMetrics)
            .Select(workout => CalculateEstimatedWorkoutCalories(workout, bmr))
            .Where(calories => calories > 0)
            .ToList();
    }

    private double CalculateEstimatedWorkoutCalories(
        HistoryWorkoutItemModel workout,
        double bmr)
    {
        var exercises = workoutHistoryService.GetExercises(workout.Id).ToList();

        var sets = exercises
            .SelectMany(exercise => workoutHistoryService.GetSets(exercise.Id))
            .Where(set => set.IsCompleted && !set.IsSkipped)
            .ToList();

        if (sets.Count == 0)
            return 0;

        var totalRestMinutes = sets.Sum(set => Math.Max(0, set.RestSeconds)) / 60d;
        var activeMinutes = Math.Max(1, workout.DurationMinutes - totalRestMinutes);
        var met = EstimateResistanceTrainingMet(exercises, sets, activeMinutes);
        var restingKcalPerMinute = bmr / 1440d;

        return Math.Max(0, met - 1d) * restingKcalPerMinute * activeMinutes;
    }

    private static double EstimateResistanceTrainingMet(
        IReadOnlyList<HistoryExerciseItemModel> exercises,
        IReadOnlyList<HistorySetItemModel> sets,
        double activeMinutes)
    {
        var completedSets = sets.Count;
        var totalReps = sets.Sum(set => Math.Max(0, set.Reps));
        var totalVolumeKg = sets.Sum(set => Math.Max(0, set.Reps) * Math.Max(0, set.WeightKg ?? 0));
        var volumeDensity = activeMinutes <= 0 ? 0 : totalVolumeKg / activeMinutes;
        var setDensity = activeMinutes <= 0 ? 0 : completedSets / activeMinutes;
        var hasCompoundWork = exercises.Any(exercise => IsCompound(exercise) || exercise.PrimaryMuscleCategories.Count > 1 || exercise.PrimaryMuscles.Count > 2);
        var averageReps = completedSets == 0 ? 0 : totalReps / (double)completedSets;

        var met = 3.5d;

        if (hasCompoundWork)
            met += 0.6d;

        if (volumeDensity >= 700)
            met += 0.8d;
        else if (volumeDensity >= 400)
            met += 0.5d;
        else if (volumeDensity >= 200)
            met += 0.25d;

        if (setDensity >= 0.5)
            met += 0.5d;
        else if (setDensity >= 0.35)
            met += 0.25d;

        if (averageReps <= 6 && volumeDensity >= 250)
            met += 0.35d;

        return Math.Clamp(met, 3.0d, 6.5d);
    }

    private static bool IsCompound(HistoryExerciseItemModel exercise)
    {
        return exercise.Mechanic?.ToString().Contains("Compound", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static List<DashboardPointMetric> BuildActivityTimeline(
        IReadOnlyList<HistoryWorkoutItemModel> history,
        DateTime from,
        DateTime to)
    {
        var days = Math.Max(1, (to.Date - from.Date).Days + 1);

        if (days <= 31)
        {
            return Enumerable
                .Range(0, days)
                .Select(offset =>
                {
                    var day = from.Date.AddDays(offset);

                    return new DashboardPointMetric
                    {
                        Label = day.ToString("dd MMM", CultureInfo.CurrentCulture),
                        Value = history.Count(workout => workout.CompletedAt.Date == day)
                    };
                })
                .ToList();
        }

        var buckets = new List<DashboardPointMetric>();
        var cursor = new DateTime(from.Year, from.Month, 1);
        var endMonth = new DateTime(to.Year, to.Month, 1);

        while (cursor <= endMonth)
        {
            var bucketStart = cursor;
            var bucketEnd = cursor.AddMonths(1).AddTicks(-1);

            buckets.Add(new DashboardPointMetric
            {
                Label = cursor.ToString("MMM", CultureInfo.CurrentCulture),
                Value = history.Count(workout => workout.CompletedAt >= bucketStart && workout.CompletedAt <= bucketEnd)
            });

            cursor = cursor.AddMonths(1);
        }

        return buckets;
    }

    private List<DashboardCategoryMetric> BuildPreferredMuscleGroups(
        IReadOnlyList<HistoryExerciseItemModel> exercises)
    {
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in exercises)
        {
            var completedSetCount = CountCompletedSets(exercise.Id);

            if (completedSetCount <= 0)
                continue;

            foreach (var category in exercise.PrimaryMuscleCategories)
            {
                var label = category.Trim();

                if (string.IsNullOrWhiteSpace(label))
                    continue;

                values[label] = values.TryGetValue(label, out var current)
                    ? current + completedSetCount
                    : completedSetCount;
            }
        }

        return values
            .Select(pair => new DashboardCategoryMetric
            {
                Label = pair.Key,
                Value = pair.Value
            })
            .OrderByDescending(metric => metric.Value)
            .ThenBy(metric => metric.Label)
            .Take(6)
            .ToList();
    }

    private List<DashboardCategoryMetric> BuildPreferredEquipment(
        IReadOnlyList<HistoryExerciseItemModel> exercises)
    {
        return BuildSetWeightedCategoryMetric(
            exercises,
            exercise => ToDisplayName(exercise.Equipment));
    }

    private List<DashboardCategoryMetric> BuildPreferredMechanics(
        IReadOnlyList<HistoryExerciseItemModel> exercises)
    {
        return BuildSetWeightedCategoryMetric(
            exercises,
            exercise => ToDisplayName(exercise.Mechanic));
    }

    private List<DashboardCategoryMetric> BuildSetWeightedCategoryMetric(
        IReadOnlyList<HistoryExerciseItemModel> exercises,
        Func<HistoryExerciseItemModel, string> labelSelector)
    {
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in exercises)
        {
            var completedSetCount = CountCompletedSets(exercise.Id);

            if (completedSetCount <= 0)
                continue;

            var label = labelSelector(exercise).Trim();

            if (string.IsNullOrWhiteSpace(label))
                continue;

            values[label] = values.TryGetValue(label, out var current)
                ? current + completedSetCount
                : completedSetCount;
        }

        return values
            .Select(pair => new DashboardCategoryMetric
            {
                Label = pair.Key,
                Value = pair.Value
            })
            .OrderByDescending(metric => metric.Value)
            .ThenBy(metric => metric.Label)
            .Take(6)
            .ToList();
    }

    private int CountCompletedSets(Guid historyExerciseId)
    {
        return workoutHistoryService
            .GetSets(historyExerciseId)
            .Count(set => set.IsCompleted && !set.IsSkipped);
    }

    private DashboardTargetProgress BuildTargetProgress(
        string rangeId,
        int actualSessions,
        double actualCalories)
    {
        var settings = userSettingsService.GetOrCreate();
        var weekMultiplier = ResolveTargetWeekMultiplier(rangeId);
        var targetSessions = Math.Max(0, settings.WeeklyGoalSessions * weekMultiplier);
        var targetCalories = Math.Max(0, settings.WeeklyCalorieTarget * weekMultiplier);

        return new DashboardTargetProgress
        {
            WeekMultiplier = weekMultiplier,
            ActualSessions = Math.Max(0, actualSessions),
            TargetSessions = targetSessions,
            ActualCalories = Math.Max(0, actualCalories),
            TargetCalories = targetCalories
        };
    }

    private static int ResolveTargetWeekMultiplier(string rangeId)
    {
        return rangeId switch
        {
            "month" => 4,
            "3m" => 12,
            "6m" => 24,
            "year" => 52,
            _ => 1
        };
    }

    private bool TryGetUserProfileForCalories(
        out double weightKg,
        out double heightCm,
        out int age,
        out DashboardSex sex)
    {
        weightKg = 0;
        heightCm = 0;
        age = 0;
        sex = DashboardSex.Unknown;

        var settings = userSettingsService.GetOrCreate();

        if (!int.TryParse(settings.Age?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out age))
            return false;

        if (age <= 0)
            return false;

        if (!TryParseFlexibleDouble(settings.Weight, out var weight))
            return false;

        if (!TryParseFlexibleDouble(settings.Height, out var height))
            return false;

        if (weight <= 0 || height <= 0)
            return false;

        var isImperial = settings.Units.ToString().Contains("Imperial", StringComparison.OrdinalIgnoreCase);

        weightKg = isImperial ? weight * 0.45359237d : weight;
        heightCm = isImperial ? height * 2.54d : height;
        sex = ParseSex(settings.Gender);

        return sex is DashboardSex.Male or DashboardSex.Female;
    }

    private static double CalculateBmrMifflin(
        double weightKg,
        double heightCm,
        int age,
        DashboardSex sex)
    {
        var bmr = 10.0 * weightKg + 6.25 * heightCm - 5.0 * age;

        return sex == DashboardSex.Male
            ? bmr + 5.0
            : bmr - 161.0;
    }

    private static DashboardSex ParseSex(GenderOption gender)
    {
        var normalized = Normalize(gender.ToString());

        if (normalized is "male" or "man")
            return DashboardSex.Male;

        if (normalized is "female" or "woman")
            return DashboardSex.Female;

        return DashboardSex.Unknown;
    }

    private static double CalculateSessionsPerWeek(int completedWorkoutCount, DateTime from, DateTime to)
    {
        var days = Math.Max(1, (to.Date - from.Date).TotalDays + 1);

        return completedWorkoutCount / (days / 7d);
    }

    private static bool TryParseFlexibleDouble(string? value, out double result)
    {
        var text = value?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0;
            return false;
        }

        var normalized = text.Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
    }

    private static string ToDisplayName(ExerciseEquipment? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : "Unknown";
    }

    private static string ToDisplayName(ExerciseMechanic? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : "Unknown";
    }

    private static string Normalize(string value)
    {
        return new string(
            value
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
    }

    private enum DashboardSex
    {
        Unknown,
        Male,
        Female
    }
}

public sealed class DashboardMetrics
{
    public double AverageEstimatedCaloriesBurned { get; init; }

    public double TotalEstimatedCaloriesBurned { get; init; }

    public double AverageWorkoutMinutes { get; init; }

    public double AverageSessionsPerWeek { get; init; }

    public DashboardTargetProgress TargetProgress { get; init; } = new();

    public IReadOnlyList<DashboardPointMetric> ActivityTimeline { get; init; } = [];

    public IReadOnlyList<DashboardCategoryMetric> PreferredMuscleGroups { get; init; } = [];

    public IReadOnlyList<DashboardCategoryMetric> PreferredEquipment { get; init; } = [];

    public IReadOnlyList<DashboardCategoryMetric> PreferredMechanics { get; init; } = [];
}

public sealed class DashboardTargetProgress
{
    public int WeekMultiplier { get; init; }

    public int ActualSessions { get; init; }

    public int TargetSessions { get; init; }

    public double ActualCalories { get; init; }

    public double TargetCalories { get; init; }
}

public sealed class DashboardPointMetric
{
    public string Label { get; init; } = string.Empty;

    public double Value { get; init; }
}

public sealed class DashboardCategoryMetric
{
    public string Label { get; init; } = string.Empty;

    public double Value { get; init; }
}