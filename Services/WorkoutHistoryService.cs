using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Repositories.History;

namespace XerSize.Services;

public sealed class WorkoutHistoryService
{
    private readonly HistoryWorkoutItemRepository historyWorkouts;
    private readonly HistoryExerciseItemRepository historyExercises;
    private readonly HistorySetItemRepository historySets;

    public WorkoutHistoryService(
        HistoryWorkoutItemRepository historyWorkouts,
        HistoryExerciseItemRepository historyExercises,
        HistorySetItemRepository historySets)
    {
        this.historyWorkouts = historyWorkouts;
        this.historyExercises = historyExercises;
        this.historySets = historySets;
    }

    public bool Exists(Guid historyWorkoutId)
    {
        return historyWorkouts.GetById(historyWorkoutId) is not null;
    }

    public IReadOnlyList<HistoryWorkoutItemModel> GetHistory()
    {
        return historyWorkouts.Get(query => query.OrderByDescending(workout => workout.CompletedAt));
    }

    public IReadOnlyList<HistoryWorkoutItemModel> GetHistory(DateTime from, DateTime to)
    {
        return historyWorkouts.Get(query =>
            query
                .Where(workout => workout.CompletedAt >= from && workout.CompletedAt <= to)
                .OrderByDescending(workout => workout.CompletedAt));
    }

    public IReadOnlyList<HistoryExerciseItemModel> GetExercises(Guid historyWorkoutId)
    {
        return historyExercises.Get(query =>
            query
                .Where(exercise => exercise.HistoryWorkoutId == historyWorkoutId)
                .OrderBy(exercise => exercise.CompletedAt));
    }

    public IReadOnlyList<HistorySetItemModel> GetSets(Guid historyExerciseId)
    {
        return historySets.Get(query =>
            query
                .Where(set => set.HistoryExerciseId == historyExerciseId)
                .OrderBy(set => set.SortNumber));
    }

    public HistoryWorkoutItemModel CreateFromActiveWorkout(
        ActiveWorkoutSessionModel activeSession,
        IReadOnlyList<ActiveWorkoutExerciseModel> activeExercises,
        Func<Guid, IReadOnlyList<ActiveWorkoutSetModel>> activeSetsProvider,
        bool excludeVolumeFromMetrics,
        bool excludeCaloriesFromMetrics,
        bool excludeMetadataFromMetrics,
        string notes = "")
    {
        ArgumentNullException.ThrowIfNull(activeSession);
        ArgumentNullException.ThrowIfNull(activeExercises);
        ArgumentNullException.ThrowIfNull(activeSetsProvider);

        var completedAt = activeSession.CompletedAt ?? DateTime.Now;
        var allSets = activeExercises
            .SelectMany(exercise => activeSetsProvider(exercise.Id))
            .ToList();

        var completedSets = allSets.Where(set => set.IsCompleted).ToList();
        var skippedSets = allSets.Where(set => set.IsSkipped).ToList();

        var historyWorkout = historyWorkouts.Create(new HistoryWorkoutItemModel
        {
            WorkoutId = activeSession.WorkoutId,
            WorkoutName = activeSession.WorkoutName,
            StartedAt = activeSession.StartedAt,
            CompletedAt = completedAt,
            DurationMinutes = Math.Max(0, (int)(completedAt - activeSession.StartedAt).TotalMinutes),
            IsPartial = activeSession.IsPartial,
            PlannedSetCount = allSets.Count,
            CompletedSetCount = completedSets.Count,
            SkippedSetCount = skippedSets.Count,
            WeightKgAtTime = activeSession.WeightKgAtTime,
            AgeAtTime = activeSession.AgeAtTime,
            ExcludeVolumeFromMetrics = excludeVolumeFromMetrics,
            ExcludeCaloriesFromMetrics = excludeCaloriesFromMetrics,
            ExcludeMetadataFromMetrics = excludeMetadataFromMetrics,
            Notes = notes.Trim()
        });

        foreach (var activeExercise in activeExercises.OrderBy(exercise => exercise.SortNumber))
        {
            var historyExercise = historyExercises.Create(new HistoryExerciseItemModel
            {
                HistoryWorkoutId = historyWorkout.Id,
                CatalogExerciseId = activeExercise.CatalogExerciseId,
                WorkoutExerciseId = activeExercise.WorkoutExerciseId,
                Name = activeExercise.Name,
                Notes = activeExercise.Notes,
                ImageSource = activeExercise.ImageSource,
                TrackingMode = activeExercise.TrackingMode,
                Force = activeExercise.Force,
                BodyCategory = activeExercise.BodyCategory,
                Mechanic = activeExercise.Mechanic,
                Equipment = activeExercise.Equipment,
                LimbInvolvement = activeExercise.LimbInvolvement,
                MovementPattern = activeExercise.MovementPattern,
                PrimaryMuscleCategories = [.. activeExercise.PrimaryMuscleCategories],
                SecondaryMuscleCategories = [.. activeExercise.SecondaryMuscleCategories],
                PrimaryMuscles = [.. activeExercise.PrimaryMuscles],
                SecondaryMuscles = [.. activeExercise.SecondaryMuscles],
                CompletedAt = completedAt
            });

            foreach (var activeSet in activeSetsProvider(activeExercise.Id).OrderBy(set => set.SortNumber))
            {
                historySets.Create(new HistorySetItemModel
                {
                    HistoryExerciseId = historyExercise.Id,
                    SortNumber = activeSet.SortNumber,
                    Reps = activeSet.Reps,
                    WeightKg = activeSet.WeightKg,
                    DurationSeconds = activeSet.DurationSeconds,
                    DistanceMeters = activeSet.DistanceMeters,
                    RestSeconds = activeSet.RestSeconds,
                    IsCompleted = activeSet.IsCompleted,
                    IsSkipped = activeSet.IsSkipped,
                    CompletedAt = activeSet.IsCompleted || activeSet.IsSkipped
                        ? completedAt
                        : null
                });
            }
        }

        return historyWorkout;
    }

    public bool RemoveHistoryWorkout(Guid historyWorkoutId)
    {
        var exercises = GetExercises(historyWorkoutId);

        foreach (var exercise in exercises)
        {
            foreach (var set in GetSets(exercise.Id))
                historySets.Remove(set.Id);

            historyExercises.Remove(exercise.Id);
        }

        return historyWorkouts.Remove(historyWorkoutId);
    }

    public void ClearHistory()
    {
        historySets.Clear();
        historyExercises.Clear();
        historyWorkouts.Clear();
    }
}