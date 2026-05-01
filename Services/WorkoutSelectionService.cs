namespace XerSize.Services;

public sealed class WorkoutSelectionService
{
    public Guid? SelectedWorkoutId { get; private set; }

    public void SelectWorkout(Guid workoutId)
    {
        SelectedWorkoutId = workoutId;
    }

    public void ClearSelection()
    {
        SelectedWorkoutId = null;
    }
}
