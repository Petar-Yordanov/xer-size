namespace XerSize.Models;

public sealed class LoggedWorkoutSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoutineId { get; set; }
    public string RoutineName { get; set; } = string.Empty;

    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;

    public DateTime PerformedAt { get; set; }
    public int DurationMinutes { get; set; }
    public double EstimatedCaloriesBurned { get; set; }
    public double BodyWeightKg { get; set; }

    public List<LoggedWorkoutExercise> Exercises { get; set; } = new();

    public double TotalVolume => Exercises.Sum(x => x.TotalVolume);
    public int TotalSets => Exercises.Sum(x => x.Sets.Count);
}

public sealed class LoggedWorkoutExercise
{
    public Guid WorkoutExerciseId { get; set; }
    public string CatalogExerciseId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Force { get; set; } = string.Empty;
    public string BodyCategory { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public List<string> PrimaryMuscles { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public List<string> PrimaryMuscleCategories { get; set; } = new();
    public List<string> SecondaryMuscleCategories { get; set; } = new();
    public string LimbInvolvement { get; set; } = string.Empty;
    public string? MovementPattern { get; set; }

    public List<LoggedSet> Sets { get; set; } = new();

    public double TotalVolume => Sets.Sum(x => (x.WeightKg ?? 0) * (x.Reps ?? 0));
}

public sealed class LoggedSet
{
    public int Order { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public int? RestSeconds { get; set; }
}