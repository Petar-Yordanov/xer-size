using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IActiveWorkoutService
{
    Task<ActiveWorkoutSession> StartAsync(Guid routineId, Guid workoutId);
    Task<ActiveWorkoutSession?> GetCurrentAsync();

    Task SelectExerciseAsync(Guid sessionId, int exerciseIndex);

    Task StartSetAsync(Guid sessionId, int exerciseIndex, int setOrder);
    Task CompleteSetAsync(
        Guid sessionId,
        int exerciseIndex,
        int setOrder,
        int? actualReps,
        double? actualWeightKg,
        int? actualDurationSeconds);

    Task SkipSetAsync(Guid sessionId, int exerciseIndex, int setOrder);
    Task SkipRestAsync(Guid sessionId);

    Task<LoggedWorkoutSession> FinishAsync(Guid sessionId);
    Task CancelAsync(Guid sessionId);
}