using System.Globalization;
using XerSize.Core;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;

namespace XerSize.Services;

public sealed class DashboardStatisticsService
{
    private const double FallbackWeightKg = 81d;
    private const double FallbackHeightCm = 180d;
    private const int FallbackAge = 27;

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
        var rawHistory = workoutHistoryService.GetHistory(from, to);

        var countableHistory = rawHistory
            .Where(workout => !workout.ExcludeMetadataFromMetrics)
            .ToList();

        var calorieHistory = rawHistory
            .Where(workout => !workout.ExcludeCaloriesFromMetrics)
            .ToList();

        var completedWorkoutCount = countableHistory.Count;
        var estimatedCaloriesByWorkout = CalculateEstimatedCaloriesByWorkout(calorieHistory);
        var totalEstimatedCalories = estimatedCaloriesByWorkout.Sum();

        var historyExercises = countableHistory
            .SelectMany(workout => workoutHistoryService.GetExercises(workout.Id))
            .ToList();

        if (rawHistory.Count == 0)
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

        var averageCalories = estimatedCaloriesByWorkout.Count == 0
            ? 0
            : estimatedCaloriesByWorkout.Average();

        var averageWorkoutMinutes = countableHistory.Count == 0
            ? 0
            : countableHistory.Average(workout => Math.Max(0, workout.DurationMinutes));

        var averageSessionsPerWeek = CalculateSessionsPerWeek(completedWorkoutCount, from, to);

        return new DashboardMetrics
        {
            AverageEstimatedCaloriesBurned = averageCalories,
            TotalEstimatedCaloriesBurned = totalEstimatedCalories,
            AverageWorkoutMinutes = averageWorkoutMinutes,
            AverageSessionsPerWeek = averageSessionsPerWeek,
            TargetProgress = BuildTargetProgress(rangeId, completedWorkoutCount, totalEstimatedCalories),
            ActivityTimeline = BuildActivityTimeline(countableHistory, from, to),
            PreferredMuscleGroups = BuildPreferredMuscleGroups(historyExercises),
            PreferredEquipment = BuildPreferredEquipment(historyExercises),
            PreferredMechanics = BuildPreferredMechanics(historyExercises)
        };
    }

    public double CalculateEstimatedWorkoutCalories(HistoryWorkoutItemModel workout)
    {
        if (workout.ExcludeCaloriesFromMetrics)
            return 0;

        var inputs = GetCalorieInputs(workout);
        var bmr = CalorieCalculator.CalculateBmrMifflin(
            inputs.WeightKg,
            inputs.HeightCm,
            inputs.Age,
            inputs.Sex);

        return CalculateEstimatedWorkoutCalories(workout, bmr, inputs.WeightKg);
    }

    private List<double> CalculateEstimatedCaloriesByWorkout(IReadOnlyList<HistoryWorkoutItemModel> history)
    {
        var results = new List<double>();

        foreach (var workout in history.Where(workout => !workout.ExcludeCaloriesFromMetrics))
        {
            var calories = CalculateEstimatedWorkoutCalories(workout);

            if (calories > 0)
                results.Add(calories);
        }

        return results;
    }

    private double CalculateEstimatedWorkoutCalories(
        HistoryWorkoutItemModel workout,
        double bmr,
        double weightKg)
    {
        var exercises = workoutHistoryService.GetExercises(workout.Id).ToList();

        if (exercises.Count == 0)
            return CalculateWholeWorkoutFallbackCalories(workout, bmr);

        var estimatedCalories = 0d;
        var hasAnyTimedCalories = false;

        foreach (var exercise in exercises)
        {
            var completedSets = workoutHistoryService
                .GetSets(exercise.Id)
                .Where(set => set.IsCompleted && !set.IsSkipped)
                .ToList();

            if (completedSets.Count == 0)
                continue;

            var calories = exercise.TrackingMode switch
            {
                ExerciseTrackingMode.Time => CalculateTimeBasedCalories(exercise, completedSets, bmr, weightKg),
                ExerciseTrackingMode.TimeAndDistance => CalculateTimeAndDistanceCalories(exercise, completedSets, bmr, weightKg),
                _ => 0
            };

            if (calories > 0)
            {
                hasAnyTimedCalories = true;
                estimatedCalories += calories;
            }
        }

        var strengthExercises = exercises
            .Where(exercise => exercise.TrackingMode == ExerciseTrackingMode.Strength)
            .ToList();

        var strengthSets = strengthExercises
            .SelectMany(exercise => workoutHistoryService.GetSets(exercise.Id))
            .Where(set => set.IsCompleted && !set.IsSkipped)
            .ToList();

        if (strengthSets.Count > 0)
            estimatedCalories += CalculateStrengthCalories(workout, strengthExercises, strengthSets, bmr, weightKg);

        if (estimatedCalories <= 0 && !hasAnyTimedCalories)
            estimatedCalories = CalculateWholeWorkoutFallbackCalories(workout, bmr);

        return Math.Max(0, estimatedCalories);
    }

    private static double CalculateTimeBasedCalories(
        HistoryExerciseItemModel exercise,
        IReadOnlyList<HistorySetItemModel> sets,
        double bmr,
        double weightKg)
    {
        var activeMinutes = sets.Sum(set => Math.Max(0, set.DurationSeconds)) / 60d;

        if (activeMinutes <= 0)
            return 0;

        var met = InferMetFromExerciseName(exercise.Name, ExerciseTrackingMode.Time);

        return CalculateActiveCalories(bmr, weightKg, met, activeMinutes);
    }

    private static double CalculateTimeAndDistanceCalories(
        HistoryExerciseItemModel exercise,
        IReadOnlyList<HistorySetItemModel> sets,
        double bmr,
        double weightKg)
    {
        var activeMinutes = sets.Sum(set => Math.Max(0, set.DurationSeconds)) / 60d;
        var distanceMeters = sets.Sum(set => Math.Max(0, set.DistanceMeters ?? 0));

        if (activeMinutes <= 0)
            return 0;

        var initialMet = InferMetFromExerciseName(exercise.Name, ExerciseTrackingMode.TimeAndDistance);
        var speedKmh = distanceMeters > 0
            ? (distanceMeters / 1000d) / (activeMinutes / 60d)
            : 0;

        var adjustedMet = EstimateDistanceAdjustedMet(exercise.Name, initialMet, speedKmh);

        return CalculateActiveCalories(bmr, weightKg, adjustedMet, activeMinutes);
    }

    private static double CalculateStrengthCalories(
        HistoryWorkoutItemModel workout,
        IReadOnlyList<HistoryExerciseItemModel> exercises,
        IReadOnlyList<HistorySetItemModel> sets,
        double bmr,
        double weightKg)
    {
        var completedSets = sets.Where(set => set.IsCompleted && !set.IsSkipped).ToList();

        if (completedSets.Count == 0)
            return 0;

        var totalRestMinutes = completedSets.Sum(set => Math.Max(0, set.RestSeconds)) / 60d;
        var activeMinutes = Math.Max(1, workout.DurationMinutes - totalRestMinutes);

        if (workout.DurationMinutes <= 0)
            activeMinutes = Math.Max(1, completedSets.Count * 1.5d);

        var met = EstimateResistanceTrainingMet(exercises, completedSets, activeMinutes);

        return CalculateActiveCalories(bmr, weightKg, met, activeMinutes);
    }

    private static double CalculateWholeWorkoutFallbackCalories(
        HistoryWorkoutItemModel workout,
        double bmr)
    {
        var activeMinutes = Math.Max(0, workout.DurationMinutes);

        if (activeMinutes <= 0)
            return 0;

        var met = InferMetFromWorkoutName(workout.WorkoutName);

        return CalorieCalculator.CalculateWorkoutCaloriesActiveOnly(bmr, met, activeMinutes);
    }

    private static double CalculateActiveCalories(
        double bmr,
        double weightKg,
        double met,
        double activeMinutes)
    {
        if (activeMinutes <= 0 || met <= 1)
            return 0;

        var bmrBasedCalories = CalorieCalculator.CalculateWorkoutCaloriesActiveOnly(
            bmr,
            met,
            activeMinutes);

        if (bmrBasedCalories > 0)
            return bmrBasedCalories;

        return Math.Max(0, met - 1.0d) * 3.5d * Math.Max(0, weightKg) / 200d * activeMinutes;
    }

    private static double InferMetFromWorkoutName(string workoutName)
    {
        var name = Normalize(workoutName);

        if (ContainsAny(name, "stretch", "mobility", "yoga"))
            return 2.3;

        if (ContainsAny(name, "calisthenics", "bodyweight"))
            return 4.5;

        if (ContainsAny(name, "isolation", "strength", "weights", "lifting", "resistance"))
            return 4.0;

        if (ContainsAny(name, "walk", "treadmill"))
            return 3.5;

        if (ContainsAny(name, "run", "jog"))
            return 7.0;

        return 4.0;
    }

    private static double InferMetFromExerciseName(string exerciseName, ExerciseTrackingMode trackingMode)
    {
        var name = Normalize(exerciseName);

        if (ContainsAny(name, "stretch", "mobility", "yoga"))
            return 2.3;

        if (ContainsAny(name, "plank", "hold", "wallsit"))
            return 3.3;

        if (ContainsAny(name, "walk", "treadmillwalk", "outdoorwalk", "powerwalk"))
            return 3.5;

        if (ContainsAny(name, "run", "jog", "sprint"))
            return 7.0;

        if (ContainsAny(name, "cycling", "cycle", "bike"))
            return 6.0;

        if (ContainsAny(name, "row", "ergometer", "skierg"))
            return 7.0;

        if (ContainsAny(name, "swim"))
            return 6.0;

        if (ContainsAny(name, "stair", "climber", "versaclimber", "jacobsladder"))
            return 8.0;

        if (ContainsAny(name, "elliptical"))
            return 5.0;

        if (ContainsAny(name, "hiking", "hike", "rucking", "ruck"))
            return 6.0;

        return trackingMode switch
        {
            ExerciseTrackingMode.Time => 3.0,
            ExerciseTrackingMode.TimeAndDistance => 4.0,
            _ => 4.0
        };
    }

    private static double EstimateDistanceAdjustedMet(string exerciseName, double initialMet, double speedKmh)
    {
        if (speedKmh <= 0)
            return initialMet;

        var name = Normalize(exerciseName);

        if (ContainsAny(name, "walk", "treadmillwalk", "outdoorwalk", "powerwalk"))
        {
            if (speedKmh >= 7.0)
                return Math.Max(initialMet, 6.3);

            if (speedKmh >= 6.0)
                return Math.Max(initialMet, 5.0);

            if (speedKmh >= 5.0)
                return Math.Max(initialMet, 4.3);

            if (speedKmh >= 4.0)
                return Math.Max(initialMet, 3.5);

            return Math.Max(initialMet, 2.8);
        }

        if (ContainsAny(name, "run", "jog", "sprint"))
        {
            if (speedKmh >= 16.0)
                return Math.Max(initialMet, 14.0);

            if (speedKmh >= 13.0)
                return Math.Max(initialMet, 11.5);

            if (speedKmh >= 11.0)
                return Math.Max(initialMet, 10.0);

            if (speedKmh >= 9.5)
                return Math.Max(initialMet, 8.3);

            if (speedKmh >= 8.0)
                return Math.Max(initialMet, 7.0);

            return Math.Max(initialMet, 6.0);
        }

        if (ContainsAny(name, "cycling", "cycle", "bike"))
        {
            if (speedKmh >= 32.0)
                return Math.Max(initialMet, 12.0);

            if (speedKmh >= 25.0)
                return Math.Max(initialMet, 10.0);

            if (speedKmh >= 20.0)
                return Math.Max(initialMet, 8.0);

            if (speedKmh >= 16.0)
                return Math.Max(initialMet, 6.8);

            return Math.Max(initialMet, 4.0);
        }

        if (ContainsAny(name, "row", "ergometer"))
        {
            if (speedKmh >= 14.0)
                return Math.Max(initialMet, 10.0);

            if (speedKmh >= 10.0)
                return Math.Max(initialMet, 8.5);

            return Math.Max(initialMet, 6.0);
        }

        return initialMet;
    }

    private static double EstimateResistanceTrainingMet(
        IReadOnlyList<HistoryExerciseItemModel> exercises,
        IReadOnlyList<HistorySetItemModel> sets,
        double activeMinutes)
    {
        var completedSets = sets.Count;
        var totalReps = sets.Sum(set => Math.Max(0, set.Reps));
        var computedStrengthLoadKg = sets.Sum(set => Math.Max(0, set.Reps) * Math.Max(0, set.WeightKg ?? 0));
        var loadDensity = activeMinutes <= 0 ? 0 : computedStrengthLoadKg / activeMinutes;
        var setDensity = activeMinutes <= 0 ? 0 : completedSets / activeMinutes;
        var hasCompoundWork = exercises.Any(exercise =>
            IsCompound(exercise) ||
            exercise.PrimaryMuscleCategories.Count > 1 ||
            exercise.PrimaryMuscles.Count > 2);

        var averageReps = completedSets == 0 ? 0 : totalReps / (double)completedSets;

        var met = 3.5d;

        if (hasCompoundWork)
            met += 0.6d;

        if (loadDensity >= 700)
            met += 0.8d;
        else if (loadDensity >= 400)
            met += 0.5d;
        else if (loadDensity >= 200)
            met += 0.25d;

        if (setDensity >= 0.5)
            met += 0.5d;
        else if (setDensity >= 0.35)
            met += 0.25d;

        if (averageReps <= 6 && loadDensity >= 250)
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

    private CalorieInputs GetCalorieInputs(HistoryWorkoutItemModel workout)
    {
        var settings = userSettingsService.GetOrCreate();

        var weightKg = ResolveWeightKg(workout, settings.Weight, settings.Units);
        var heightCm = ResolveHeightCm(settings.Height, settings.Units);
        var age = ResolveAge(workout, settings.Age);
        var sex = settings.Gender == GenderOption.Female
            ? Sex.Female
            : Sex.Male;

        return new CalorieInputs(weightKg, heightCm, age, sex);
    }

    private static double ResolveWeightKg(
        HistoryWorkoutItemModel workout,
        string? settingsWeight,
        UnitSystem units)
    {
        if (workout.WeightKgAtTime.HasValue && workout.WeightKgAtTime.Value > 0)
            return workout.WeightKgAtTime.Value;

        if (TryParseFlexibleDouble(settingsWeight, out var parsedWeight) && parsedWeight > 0)
        {
            return units == UnitSystem.Imperial
                ? parsedWeight * 0.45359237d
                : parsedWeight;
        }

        return FallbackWeightKg;
    }

    private static double ResolveHeightCm(string? settingsHeight, UnitSystem units)
    {
        if (TryParseFlexibleDouble(settingsHeight, out var parsedHeight) && parsedHeight > 0)
        {
            return units == UnitSystem.Imperial
                ? parsedHeight * 2.54d
                : parsedHeight;
        }

        return FallbackHeightCm;
    }

    private static int ResolveAge(HistoryWorkoutItemModel workout, string? settingsAge)
    {
        if (workout.AgeAtTime.HasValue && workout.AgeAtTime.Value > 0)
            return workout.AgeAtTime.Value;

        if (int.TryParse(settingsAge?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var age) && age > 0)
            return age;

        return FallbackAge;
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

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(term => value.Contains(Normalize(term), StringComparison.OrdinalIgnoreCase));
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

    private readonly record struct CalorieInputs(
        double WeightKg,
        double HeightCm,
        int Age,
        Sex Sex);
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