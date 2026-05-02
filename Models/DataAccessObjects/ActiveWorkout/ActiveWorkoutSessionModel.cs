namespace XerSize.Models.DataAccessObjects.ActiveWorkout;

public sealed class ActiveWorkoutSessionModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkoutId { get; set; }

    public string WorkoutName { get; set; } = "Workout";

    public DateTime StartedAt { get; set; } = DateTime.Now;

    public DateTime? CompletedAt { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsPartial { get; set; }

    public int CurrentExerciseIndex { get; set; }

    public int RemainingRestSeconds { get; set; }

    public bool IsResting { get; set; }

    public DateTime? RestStartedAt { get; set; }

    public int RestDurationSeconds { get; set; }

    public double? WeightKgAtTime { get; set; }

    public int? AgeAtTime { get; set; }
}