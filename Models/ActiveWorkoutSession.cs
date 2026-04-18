namespace XerSize.Models;

public sealed class ActiveWorkoutSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoutineId { get; set; }
    public string RoutineName { get; set; } = string.Empty;

    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }

    public int CurrentExerciseIndex { get; set; }

    public int? CurrentRestExerciseIndex { get; set; }
    public int? CurrentRestSetOrder { get; set; }
    public DateTime? RestEndsAtUtc { get; set; }

    public List<ActiveWorkoutExercise> Exercises { get; set; } = new();
}

public sealed class ActiveWorkoutExercise
{
    public Guid WorkoutExerciseId { get; set; }
    public string CatalogExerciseId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Force { get; set; } = string.Empty;
    public string BodyCategory { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public List<string> PrimaryMuscleCategories { get; set; } = new();
    public List<string> SecondaryMuscleCategories { get; set; } = new();
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }

    public int SortOrder { get; set; }
    public ExerciseExecutionState State { get; set; } = ExerciseExecutionState.NotStarted;

    public List<ActiveWorkoutSet> Sets { get; set; } = new();
}

public sealed class ActiveWorkoutSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Order { get; set; }

    public int? PlannedReps { get; set; }
    public double? PlannedWeightKg { get; set; }
    public int? PlannedDurationSeconds { get; set; }
    public int? PlannedRestSeconds { get; set; }

    public int? ActualReps { get; set; }
    public double? ActualWeightKg { get; set; }
    public int? ActualDurationSeconds { get; set; }

    public SetExecutionState State { get; set; } = SetExecutionState.NotStarted;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public enum ExerciseExecutionState
{
    NotStarted = 0,
    Current = 1,
    Completed = 2,
    Skipped = 3
}

public enum SetExecutionState
{
    NotStarted = 0,
    Started = 1,
    Completed = 2,
    Skipped = 3
}