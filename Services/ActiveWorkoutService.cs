using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Repositories.ActiveWorkout;

namespace XerSize.Services;

public sealed class ActiveWorkoutService
{
    private readonly ActiveWorkoutSessionRepository activeSessions;
    private readonly ActiveWorkoutExerciseRepository activeExercises;
    private readonly ActiveWorkoutSetRepository activeSets;
    private readonly WorkoutService workoutService;
    private readonly WorkoutHistoryService workoutHistoryService;

    public ActiveWorkoutService(
        ActiveWorkoutSessionRepository activeSessions,
        ActiveWorkoutExerciseRepository activeExercises,
        ActiveWorkoutSetRepository activeSets,
        WorkoutService workoutService,
        WorkoutHistoryService workoutHistoryService)
    {
        this.activeSessions = activeSessions;
        this.activeExercises = activeExercises;
        this.activeSets = activeSets;
        this.workoutService = workoutService;
        this.workoutHistoryService = workoutHistoryService;
    }

    public bool Exists(Guid activeWorkoutSessionId)
    {
        return activeSessions.GetById(activeWorkoutSessionId) is not null;
    }

    public bool HasActiveWorkout()
    {
        return GetActiveWorkout() is not null;
    }

    public ActiveWorkoutSessionModel? GetActiveWorkout()
    {
        var session = activeSessions
            .Get(query =>
                query
                    .Where(session => !session.IsCompleted)
                    .OrderByDescending(session => session.StartedAt))
            .FirstOrDefault();

        if (session is not null)
            RefreshRestState(session);

        return session;
    }

    public ActiveWorkoutSessionModel? GetSession(Guid activeWorkoutSessionId)
    {
        var session = activeSessions.GetById(activeWorkoutSessionId);

        if (session is not null)
            RefreshRestState(session);

        return session;
    }

    public IReadOnlyList<ActiveWorkoutExerciseModel> GetExercises(Guid activeWorkoutSessionId)
    {
        return activeExercises.Get(query =>
            query
                .Where(exercise => exercise.ActiveWorkoutSessionId == activeWorkoutSessionId)
                .OrderBy(exercise => exercise.SortNumber));
    }

    public IReadOnlyList<ActiveWorkoutSetModel> GetSets(Guid activeWorkoutExerciseId)
    {
        return activeSets.Get(query =>
            query
                .Where(set => set.ActiveWorkoutExerciseId == activeWorkoutExerciseId)
                .OrderBy(set => set.SortNumber));
    }

    public ActiveWorkoutSessionModel StartWorkout(Guid workoutId)
    {
        var existingActiveSession = GetActiveWorkout();

        if (existingActiveSession is not null)
            return existingActiveSession;

        var workout = workoutService.GetWorkout(workoutId)
            ?? throw new ServiceValidationException("Workout does not exist.");

        var workoutExercises = workoutService.GetExercises(workoutId);

        if (workoutExercises.Count == 0)
            throw new ServiceValidationException("Cannot start a workout with no exercises.");

        var activeSession = activeSessions.Create(new ActiveWorkoutSessionModel
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name,
            StartedAt = DateTime.Now,
            IsCompleted = false,
            IsPartial = false,
            CurrentExerciseIndex = 0,
            RemainingRestSeconds = 0,
            IsResting = false,
            RestStartedAt = null,
            RestDurationSeconds = 0
        });

        foreach (var workoutExercise in workoutExercises)
        {
            var activeExercise = activeExercises.Create(CreateActiveExercise(activeSession.Id, workoutExercise));
            var plannedSets = workoutService.GetSets(workoutExercise.Id);

            foreach (var plannedSet in plannedSets)
                activeSets.Create(CreateActiveSet(activeExercise.Id, plannedSet));
        }

        return activeSession;
    }

    public void MoveToExercise(Guid activeWorkoutSessionId, int exerciseIndex)
    {
        var session = GetRequiredSession(activeWorkoutSessionId);
        var exerciseCount = GetExercises(activeWorkoutSessionId).Count;

        if (exerciseCount == 0)
        {
            session.CurrentExerciseIndex = 0;
        }
        else
        {
            session.CurrentExerciseIndex = Math.Clamp(exerciseIndex, 0, exerciseCount - 1);
        }

        activeSessions.Update(session.Id, session);
    }

    public void StartRest(Guid activeWorkoutSessionId, int restSeconds)
    {
        var session = GetRequiredSession(activeWorkoutSessionId);
        var normalizedRestSeconds = Math.Max(0, restSeconds);

        session.RestDurationSeconds = normalizedRestSeconds;
        session.RemainingRestSeconds = normalizedRestSeconds;
        session.RestStartedAt = normalizedRestSeconds > 0 ? DateTime.Now : null;
        session.IsResting = normalizedRestSeconds > 0;

        activeSessions.Update(session.Id, session);
    }

    public void TickRest(Guid activeWorkoutSessionId)
    {
        var session = GetRequiredSession(activeWorkoutSessionId);

        if (!session.IsResting)
            return;

        RefreshRestState(session);
    }

    public void StopRest(Guid activeWorkoutSessionId)
    {
        var session = GetRequiredSession(activeWorkoutSessionId);

        ClearRestState(session);

        activeSessions.Update(session.Id, session);
    }

    public void UpdateSet(
        Guid activeWorkoutSetId,
        int reps,
        double? weightKg,
        int restSeconds)
    {
        var set = GetRequiredSet(activeWorkoutSetId);

        set.Reps = Math.Max(0, reps);
        set.WeightKg = weightKg.HasValue ? Math.Max(0, weightKg.Value) : null;
        set.RestSeconds = Math.Max(0, restSeconds);

        activeSets.Update(set.Id, set);
    }

    public void CompleteSet(
        Guid activeWorkoutSetId,
        int? reps = null,
        double? weightKg = null,
        int? restSeconds = null)
    {
        var set = GetRequiredSet(activeWorkoutSetId);

        if (reps.HasValue)
            set.Reps = Math.Max(0, reps.Value);

        if (weightKg.HasValue)
            set.WeightKg = Math.Max(0, weightKg.Value);

        if (restSeconds.HasValue)
            set.RestSeconds = Math.Max(0, restSeconds.Value);

        set.IsCompleted = true;
        set.IsSkipped = false;

        activeSets.Update(set.Id, set);
    }

    public void SkipSet(Guid activeWorkoutSetId)
    {
        var set = GetRequiredSet(activeWorkoutSetId);

        set.IsCompleted = false;
        set.IsSkipped = true;

        activeSets.Update(set.Id, set);
    }

    public void UnmarkSet(Guid activeWorkoutSetId)
    {
        var set = GetRequiredSet(activeWorkoutSetId);

        set.IsCompleted = false;
        set.IsSkipped = false;

        activeSets.Update(set.Id, set);
    }

    public void CompleteWorkout(Guid activeWorkoutSessionId, bool isPartial = false, string notes = "")
    {
        var session = GetRequiredSession(activeWorkoutSessionId);
        var workout = workoutService.GetWorkout(session.WorkoutId)
            ?? throw new ServiceValidationException("Workout template no longer exists.");

        var exercises = GetExercises(session.Id);

        if (exercises.Count == 0)
            throw new ServiceValidationException("Cannot complete an active workout with no exercises.");

        session.CompletedAt = DateTime.Now;
        session.IsCompleted = true;
        session.IsPartial = isPartial;

        ClearRestState(session);

        activeSessions.Update(session.Id, session);

        foreach (var activeExercise in exercises)
        {
            var sets = GetSets(activeExercise.Id);

            foreach (var activeSet in sets.Where(set => set.IsCompleted && !set.IsSkipped))
            {
                workoutService.ApplyCompletedActiveSetToTemplate(
                    activeExercise.WorkoutExerciseId,
                    activeSet.SortNumber,
                    activeSet.Reps,
                    activeSet.WeightKg,
                    activeSet.RestSeconds);
            }
        }

        workoutHistoryService.CreateFromActiveWorkout(
            session,
            exercises,
            GetSets,
            workout.ExcludeVolumeFromMetrics,
            workout.ExcludeCaloriesFromMetrics,
            workout.ExcludeMetadataFromMetrics,
            notes);
    }

    public bool CancelWorkout(Guid activeWorkoutSessionId)
    {
        var exercises = GetExercises(activeWorkoutSessionId);

        foreach (var exercise in exercises)
        {
            foreach (var set in GetSets(exercise.Id))
                activeSets.Remove(set.Id);

            activeExercises.Remove(exercise.Id);
        }

        return activeSessions.Remove(activeWorkoutSessionId);
    }

    private ActiveWorkoutSessionModel GetRequiredSession(Guid activeWorkoutSessionId)
    {
        var session = activeSessions.GetById(activeWorkoutSessionId)
            ?? throw new ServiceValidationException("Active workout session does not exist.");

        RefreshRestState(session);

        return session;
    }

    private ActiveWorkoutSetModel GetRequiredSet(Guid activeWorkoutSetId)
    {
        return activeSets.GetById(activeWorkoutSetId)
            ?? throw new ServiceValidationException("Active workout set does not exist.");
    }

    private void RefreshRestState(ActiveWorkoutSessionModel session)
    {
        if (!session.IsResting)
            return;

        if (session.RestStartedAt is null || session.RestDurationSeconds <= 0)
        {
            ClearRestState(session);
            activeSessions.Update(session.Id, session);
            return;
        }

        var elapsedSeconds = (int)Math.Floor((DateTime.Now - session.RestStartedAt.Value).TotalSeconds);
        var remainingSeconds = Math.Max(0, session.RestDurationSeconds - elapsedSeconds);

        if (remainingSeconds == session.RemainingRestSeconds && remainingSeconds > 0)
            return;

        session.RemainingRestSeconds = remainingSeconds;

        if (remainingSeconds <= 0)
            ClearRestState(session);

        activeSessions.Update(session.Id, session);
    }

    private static void ClearRestState(ActiveWorkoutSessionModel session)
    {
        session.RemainingRestSeconds = 0;
        session.IsResting = false;
        session.RestStartedAt = null;
        session.RestDurationSeconds = 0;
    }

    private static ActiveWorkoutExerciseModel CreateActiveExercise(
        Guid activeWorkoutSessionId,
        WorkoutExerciseItemModel workoutExercise)
    {
        return new ActiveWorkoutExerciseModel
        {
            ActiveWorkoutSessionId = activeWorkoutSessionId,
            WorkoutExerciseId = workoutExercise.Id,
            CatalogExerciseId = workoutExercise.CatalogExerciseId,
            SortNumber = workoutExercise.SortNumber,
            Name = workoutExercise.Name,
            ImageSource = workoutExercise.ImageSource,
            Notes = workoutExercise.Notes,
            Force = workoutExercise.Force,
            BodyCategory = workoutExercise.BodyCategory,
            Mechanic = workoutExercise.Mechanic,
            Equipment = workoutExercise.Equipment,
            LimbInvolvement = workoutExercise.LimbInvolvement,
            MovementPattern = workoutExercise.MovementPattern,
            PrimaryMuscleCategories = [.. workoutExercise.PrimaryMuscleCategories],
            SecondaryMuscleCategories = [.. workoutExercise.SecondaryMuscleCategories],
            PrimaryMuscles = [.. workoutExercise.PrimaryMuscles],
            SecondaryMuscles = [.. workoutExercise.SecondaryMuscles]
        };
    }

    private static ActiveWorkoutSetModel CreateActiveSet(
        Guid activeWorkoutExerciseId,
        WorkoutSetModel workoutSet)
    {
        return new ActiveWorkoutSetModel
        {
            ActiveWorkoutExerciseId = activeWorkoutExerciseId,
            SortNumber = workoutSet.SortNumber,
            Reps = workoutSet.Reps,
            WeightKg = workoutSet.WeightKg,
            RestSeconds = workoutSet.RestSeconds,
            IsCompleted = false,
            IsSkipped = false
        };
    }
}