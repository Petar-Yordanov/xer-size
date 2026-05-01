namespace XerSize.Models.Presentation.Workouts;

public sealed class WorkoutExerciseReorderRequest
{
    public object? SourceItem { get; init; }

    public object? TargetItem { get; init; }

    public object? Item { get; init; }

    public int Direction { get; init; }
}