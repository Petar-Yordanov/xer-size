namespace XerSize.Models.DataAccessObjects.Workouts;

public sealed class WorkoutSetModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkoutExerciseId { get; set; }

    public int SortNumber { get; set; }

    public int Reps { get; set; }

    public double? WeightKg { get; set; }

    public int DurationSeconds { get; set; }

    public double? DistanceMeters { get; set; }

    public int RestSeconds { get; set; }
}