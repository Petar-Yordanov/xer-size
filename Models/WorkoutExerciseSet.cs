namespace XerSize.Models;

public sealed class WorkoutExerciseSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Order { get; set; }

    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
}