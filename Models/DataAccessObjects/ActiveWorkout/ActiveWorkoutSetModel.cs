namespace XerSize.Models.DataAccessObjects.ActiveWorkout;

public sealed class ActiveWorkoutSetModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ActiveWorkoutExerciseId { get; set; }

    public int SortNumber { get; set; }

    public int Reps { get; set; }

    public double? WeightKg { get; set; }

    public int DurationSeconds { get; set; }

    public double? DistanceMeters { get; set; }

    public int RestSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsSkipped { get; set; }
}