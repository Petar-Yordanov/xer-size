using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IRoutineService
{
    Task<IReadOnlyList<Routine>> GetAllAsync();

    Task<Routine> CreateRoutineAsync(string name);
    Task RenameRoutineAsync(Guid routineId, string newName);
    Task<Routine> DuplicateRoutineAsync(Guid routineId);
    Task DeleteRoutineAsync(Guid routineId);

    Task<Workout> AddWorkoutAsync(Guid routineId, string workoutName);
    Task RenameWorkoutAsync(Guid routineId, Guid workoutId, string newName);
    Task<Workout> DuplicateWorkoutAsync(Guid routineId, Guid workoutId);
    Task DeleteWorkoutAsync(Guid routineId, Guid workoutId);

    Task<WorkoutExercise?> GetExerciseAsync(Guid routineId, Guid workoutId, Guid workoutExerciseId);

    Task<WorkoutExercise> AddExerciseAsync(Guid routineId, Guid workoutId, string catalogExerciseId);
    Task<WorkoutExercise> AddExerciseAsync(Guid routineId, Guid workoutId, WorkoutExercise exercise);
    Task UpdateExerciseAsync(Guid routineId, Guid workoutId, WorkoutExercise exercise);
    Task DeleteExerciseAsync(Guid routineId, Guid workoutId, Guid workoutExerciseId);
}