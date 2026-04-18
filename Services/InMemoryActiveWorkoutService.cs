using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryActiveWorkoutService : IActiveWorkoutService
{
    private readonly IRoutineService _routineService;
    private ActiveWorkoutSession? _currentSession;

    public InMemoryActiveWorkoutService(IRoutineService routineService)
    {
        _routineService = routineService;
    }

    public async Task<ActiveWorkoutSession> StartAsync(Guid routineId, Guid workoutId)
    {
        var routines = await _routineService.GetAllAsync();
        var routine = routines.FirstOrDefault(x => x.Id == routineId)
            ?? throw new InvalidOperationException("Routine was not found.");

        var workout = routine.Workouts.FirstOrDefault(x => x.Id == workoutId)
            ?? throw new InvalidOperationException("Workout was not found.");

        var session = new ActiveWorkoutSession
        {
            RoutineId = routine.Id,
            RoutineName = routine.Name,
            WorkoutId = workout.Id,
            WorkoutName = workout.Name,
            StartedAtUtc = DateTime.UtcNow,
            CurrentExerciseIndex = 0,
            Exercises = workout.Exercises
                .OrderBy(x => x.SortOrder)
                .Select(CloneToActiveExercise)
                .ToList()
        };

        if (session.Exercises.Count > 0)
            session.Exercises[0].State = ExerciseExecutionState.Current;

        _currentSession = session;
        return CloneSession(session);
    }

    public Task<ActiveWorkoutSession?> GetCurrentAsync()
    {
        return Task.FromResult(_currentSession is null ? null : CloneSession(_currentSession));
    }

    public Task SelectExerciseAsync(Guid sessionId, int exerciseIndex)
    {
        var session = GetRequiredSession(sessionId);

        if (exerciseIndex < 0 || exerciseIndex >= session.Exercises.Count)
            return Task.CompletedTask;

        session.CurrentExerciseIndex = exerciseIndex;

        for (int i = 0; i < session.Exercises.Count; i++)
        {
            var exercise = session.Exercises[i];

            if (exercise.State is ExerciseExecutionState.Completed or ExerciseExecutionState.Skipped)
                continue;

            exercise.State = i == exerciseIndex
                ? ExerciseExecutionState.Current
                : ExerciseExecutionState.NotStarted;
        }

        return Task.CompletedTask;
    }

    public Task StartSetAsync(Guid sessionId, int exerciseIndex, int setOrder)
    {
        var session = GetRequiredSession(sessionId);
        var exercise = GetRequiredExercise(session, exerciseIndex);
        var set = GetRequiredSet(exercise, setOrder);

        if (set.State is SetExecutionState.Completed or SetExecutionState.Skipped)
            return Task.CompletedTask;

        session.CurrentExerciseIndex = exerciseIndex;

        for (int i = 0; i < session.Exercises.Count; i++)
        {
            var item = session.Exercises[i];

            if (item.State is ExerciseExecutionState.Completed or ExerciseExecutionState.Skipped)
                continue;

            item.State = i == exerciseIndex
                ? ExerciseExecutionState.Current
                : ExerciseExecutionState.NotStarted;
        }

        set.State = SetExecutionState.Started;
        set.StartedAtUtc = DateTime.UtcNow;

        var restSeconds = Math.Max(0, set.PlannedRestSeconds ?? 0);
        if (restSeconds > 0)
        {
            session.CurrentRestExerciseIndex = exerciseIndex;
            session.CurrentRestSetOrder = setOrder;
            session.RestEndsAtUtc = DateTime.UtcNow.AddSeconds(restSeconds);
        }
        else
        {
            session.CurrentRestExerciseIndex = null;
            session.CurrentRestSetOrder = null;
            session.RestEndsAtUtc = null;
        }

        return Task.CompletedTask;
    }

    public Task CompleteSetAsync(
        Guid sessionId,
        int exerciseIndex,
        int setOrder,
        int? actualReps,
        double? actualWeightKg,
        int? actualDurationSeconds)
    {
        var session = GetRequiredSession(sessionId);
        var exercise = GetRequiredExercise(session, exerciseIndex);
        var set = GetRequiredSet(exercise, setOrder);

        if (set.State == SetExecutionState.Skipped)
            return Task.CompletedTask;

        set.ActualReps = actualReps;
        set.ActualWeightKg = actualWeightKg;
        set.ActualDurationSeconds = actualDurationSeconds;
        set.State = SetExecutionState.Completed;
        set.CompletedAtUtc = DateTime.UtcNow;

        if (session.CurrentRestExerciseIndex == exerciseIndex && session.CurrentRestSetOrder == setOrder)
        {
            session.CurrentRestExerciseIndex = null;
            session.CurrentRestSetOrder = null;
            session.RestEndsAtUtc = null;
        }

        if (exercise.Sets.All(x => x.State is SetExecutionState.Completed or SetExecutionState.Skipped))
        {
            exercise.State = ExerciseExecutionState.Completed;

            var nextExerciseIndex = session.Exercises.FindIndex(
                exerciseIndex + 1,
                x => x.State is not ExerciseExecutionState.Completed and not ExerciseExecutionState.Skipped);

            if (nextExerciseIndex >= 0)
            {
                session.CurrentExerciseIndex = nextExerciseIndex;
                session.Exercises[nextExerciseIndex].State = ExerciseExecutionState.Current;
            }
        }

        return Task.CompletedTask;
    }

    public Task SkipSetAsync(Guid sessionId, int exerciseIndex, int setOrder)
    {
        var session = GetRequiredSession(sessionId);
        var exercise = GetRequiredExercise(session, exerciseIndex);
        var set = GetRequiredSet(exercise, setOrder);

        if (set.State == SetExecutionState.Completed)
            return Task.CompletedTask;

        set.State = SetExecutionState.Skipped;
        set.CompletedAtUtc = DateTime.UtcNow;

        if (session.CurrentRestExerciseIndex == exerciseIndex && session.CurrentRestSetOrder == setOrder)
        {
            session.CurrentRestExerciseIndex = null;
            session.CurrentRestSetOrder = null;
            session.RestEndsAtUtc = null;
        }

        if (exercise.Sets.All(x => x.State is SetExecutionState.Completed or SetExecutionState.Skipped))
        {
            exercise.State = ExerciseExecutionState.Completed;
        }

        return Task.CompletedTask;
    }

    public Task SkipRestAsync(Guid sessionId)
    {
        var session = GetRequiredSession(sessionId);
        session.CurrentRestExerciseIndex = null;
        session.CurrentRestSetOrder = null;
        session.RestEndsAtUtc = null;
        return Task.CompletedTask;
    }

    public Task<LoggedWorkoutSession> FinishAsync(Guid sessionId)
    {
        var session = GetRequiredSession(sessionId);

        var finishedAt = DateTime.UtcNow;
        session.FinishedAtUtc = finishedAt;

        var durationMinutes = Math.Max(
            1,
            (int)Math.Ceiling((finishedAt - session.StartedAtUtc).TotalMinutes));

        var logged = new LoggedWorkoutSession
        {
            RoutineId = session.RoutineId,
            RoutineName = session.RoutineName,
            WorkoutId = session.WorkoutId,
            WorkoutName = session.WorkoutName,
            PerformedAt = finishedAt.ToLocalTime(),
            DurationMinutes = durationMinutes,
            EstimatedCaloriesBurned = Math.Round(durationMinutes * 5.0, 0),
            BodyWeightKg = 0,
            Exercises = session.Exercises.Select(ToLoggedExercise).ToList()
        };

        _currentSession = null;
        return Task.FromResult(logged);
    }

    public Task CancelAsync(Guid sessionId)
    {
        var session = GetRequiredSession(sessionId);
        if (_currentSession?.Id == session.Id)
            _currentSession = null;

        return Task.CompletedTask;
    }

    private ActiveWorkoutSession GetRequiredSession(Guid sessionId)
    {
        if (_currentSession is null || _currentSession.Id != sessionId)
            throw new InvalidOperationException("No active workout session is available.");

        return _currentSession;
    }

    private static ActiveWorkoutExercise GetRequiredExercise(ActiveWorkoutSession session, int exerciseIndex)
    {
        if (exerciseIndex < 0 || exerciseIndex >= session.Exercises.Count)
            throw new InvalidOperationException("Exercise index is out of range.");

        return session.Exercises[exerciseIndex];
    }

    private static ActiveWorkoutSet GetRequiredSet(ActiveWorkoutExercise exercise, int setOrder)
    {
        var set = exercise.Sets.FirstOrDefault(x => x.Order == setOrder);
        if (set is null)
            throw new InvalidOperationException("Set was not found.");

        return set;
    }

    private static ActiveWorkoutExercise CloneToActiveExercise(WorkoutExercise source)
    {
        return new ActiveWorkoutExercise
        {
            WorkoutExerciseId = source.Id,
            CatalogExerciseId = source.CatalogExerciseId,
            Name = source.Name,
            Force = source.Force,
            BodyCategory = source.BodyCategory,
            Mechanic = source.Mechanic,
            Equipment = source.Equipment,
            PrimaryMuscleCategories = source.PrimaryMuscleCategories.ToList(),
            SecondaryMuscleCategories = source.SecondaryMuscleCategories.ToList(),
            Notes = source.Notes,
            ImagePath = source.ImagePath,
            SortOrder = source.SortOrder,
            State = ExerciseExecutionState.NotStarted,
            Sets = source.Sets
                .OrderBy(x => x.Order)
                .Select(x => new ActiveWorkoutSet
                {
                    Order = x.Order,
                    PlannedReps = x.Reps,
                    PlannedWeightKg = x.WeightKg,
                    PlannedDurationSeconds = x.DurationSeconds,
                    PlannedRestSeconds = x.RestSeconds
                })
                .ToList()
        };
    }

    private static LoggedWorkoutExercise ToLoggedExercise(ActiveWorkoutExercise source)
    {
        return new LoggedWorkoutExercise
        {
            WorkoutExerciseId = source.WorkoutExerciseId,
            CatalogExerciseId = source.CatalogExerciseId,
            Name = source.Name,
            Force = source.Force,
            BodyCategory = source.BodyCategory,
            Mechanic = source.Mechanic,
            Equipment = source.Equipment,
            PrimaryMuscles = new List<string>(),
            SecondaryMuscles = new List<string>(),
            PrimaryMuscleCategories = source.PrimaryMuscleCategories.ToList(),
            SecondaryMuscleCategories = source.SecondaryMuscleCategories.ToList(),
            LimbInvolvement = string.Empty,
            MovementPattern = null,
            Sets = source.Sets
                .Where(x => x.State != SetExecutionState.Skipped)
                .OrderBy(x => x.Order)
                .Select(x => new LoggedSet
                {
                    Order = x.Order,
                    Reps = x.ActualReps ?? x.PlannedReps,
                    WeightKg = x.ActualWeightKg ?? x.PlannedWeightKg,
                    DurationSeconds = x.ActualDurationSeconds ?? x.PlannedDurationSeconds,
                    RestSeconds = x.PlannedRestSeconds
                })
                .ToList()
        };
    }

    private static ActiveWorkoutSession CloneSession(ActiveWorkoutSession source)
    {
        return new ActiveWorkoutSession
        {
            Id = source.Id,
            RoutineId = source.RoutineId,
            RoutineName = source.RoutineName,
            WorkoutId = source.WorkoutId,
            WorkoutName = source.WorkoutName,
            StartedAtUtc = source.StartedAtUtc,
            FinishedAtUtc = source.FinishedAtUtc,
            CurrentExerciseIndex = source.CurrentExerciseIndex,
            CurrentRestExerciseIndex = source.CurrentRestExerciseIndex,
            CurrentRestSetOrder = source.CurrentRestSetOrder,
            RestEndsAtUtc = source.RestEndsAtUtc,
            Exercises = source.Exercises.Select(x => new ActiveWorkoutExercise
            {
                WorkoutExerciseId = x.WorkoutExerciseId,
                CatalogExerciseId = x.CatalogExerciseId,
                Name = x.Name,
                Force = x.Force,
                BodyCategory = x.BodyCategory,
                Mechanic = x.Mechanic,
                Equipment = x.Equipment,
                PrimaryMuscleCategories = x.PrimaryMuscleCategories.ToList(),
                SecondaryMuscleCategories = x.SecondaryMuscleCategories.ToList(),
                Notes = x.Notes,
                ImagePath = x.ImagePath,
                SortOrder = x.SortOrder,
                State = x.State,
                Sets = x.Sets.Select(s => new ActiveWorkoutSet
                {
                    Id = s.Id,
                    Order = s.Order,
                    PlannedReps = s.PlannedReps,
                    PlannedWeightKg = s.PlannedWeightKg,
                    PlannedDurationSeconds = s.PlannedDurationSeconds,
                    PlannedRestSeconds = s.PlannedRestSeconds,
                    ActualReps = s.ActualReps,
                    ActualWeightKg = s.ActualWeightKg,
                    ActualDurationSeconds = s.ActualDurationSeconds,
                    State = s.State,
                    StartedAtUtc = s.StartedAtUtc,
                    CompletedAtUtc = s.CompletedAtUtc
                }).ToList()
            }).ToList()
        };
    }
}