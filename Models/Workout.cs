namespace XerSize.Models;

public sealed class Workout
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<WorkoutExercise> Exercises { get; set; } = new();
}