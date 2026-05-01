using XerSize.Models.DataAccessObjects.Catalog;
using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Models.Definitions;
using XerSize.Repositories.Workouts;

namespace XerSize.Services;

public sealed class WorkoutService
{
    private readonly WorkoutRepository workouts;
    private readonly WorkoutExerciseItemRepository workoutExercises;
    private readonly WorkoutSetRepository workoutSets;
    private readonly ExerciseCatalogService exerciseCatalogService;

    public WorkoutService(
        WorkoutRepository workouts,
        WorkoutExerciseItemRepository workoutExercises,
        WorkoutSetRepository workoutSets,
        ExerciseCatalogService exerciseCatalogService)
    {
        this.workouts = workouts;
        this.workoutExercises = workoutExercises;
        this.workoutSets = workoutSets;
        this.exerciseCatalogService = exerciseCatalogService;
    }

    public bool Exists(Guid workoutId)
    {
        return workouts.GetById(workoutId) is not null;
    }

    public WorkoutModel? GetWorkout(Guid workoutId)
    {
        return workouts.GetById(workoutId);
    }

    public IReadOnlyList<WorkoutModel> GetWorkouts()
    {
        return workouts.Get(query => query.OrderBy(workout => workout.SortNumber));
    }

    public IReadOnlyList<WorkoutExerciseItemModel> GetExercises(Guid workoutId)
    {
        return workoutExercises.Get(query =>
            query
                .Where(exercise => exercise.WorkoutId == workoutId)
                .OrderBy(exercise => exercise.SortNumber));
    }

    public IReadOnlyList<WorkoutSetModel> GetSets(Guid workoutExerciseId)
    {
        return workoutSets.Get(query =>
            query
                .Where(set => set.WorkoutExerciseId == workoutExerciseId)
                .OrderBy(set => set.SortNumber));
    }

    public WorkoutModel CreateWorkout(
        string name,
        TrainingType trainingType = TrainingType.Strength,
        bool excludeVolumeFromMetrics = false,
        bool excludeMetadataFromMetrics = false,
        bool excludeCaloriesFromMetrics = false)
    {
        ValidateWorkoutName(name);

        var nextSortNumber = workouts.Count();

        return workouts.Create(new WorkoutModel
        {
            Name = name.Trim(),
            SortNumber = nextSortNumber,
            TrainingType = trainingType,
            ExcludeVolumeFromMetrics = excludeVolumeFromMetrics,
            ExcludeCaloriesFromMetrics = excludeCaloriesFromMetrics,
            ExcludeMetadataFromMetrics = excludeMetadataFromMetrics
        });
    }

    public WorkoutModel EnsureWorkout(
        string name,
        TrainingType trainingType = TrainingType.Strength)
    {
        var existingWorkout = workouts
            .Get(query => query.Where(workout => workout.Name == name))
            .FirstOrDefault();

        return existingWorkout ?? CreateWorkout(name, trainingType);
    }

    public void RenameWorkout(Guid workoutId, string name)
    {
        ValidateWorkoutExists(workoutId);
        ValidateWorkoutName(name);

        var workout = workouts.GetById(workoutId)!;
        workout.Name = name.Trim();

        workouts.Update(workoutId, workout);
    }

    public void UpdateWorkoutSettings(
        Guid workoutId,
        TrainingType trainingType,
        bool excludeVolumeFromMetrics,
        bool excludeCaloriesFromMetrics,
        bool excludeMetadataFromMetrics)
    {
        ValidateWorkoutExists(workoutId);

        var workout = workouts.GetById(workoutId)!;
        workout.TrainingType = trainingType;
        workout.ExcludeVolumeFromMetrics = excludeVolumeFromMetrics;
        workout.ExcludeCaloriesFromMetrics = excludeCaloriesFromMetrics;
        workout.ExcludeMetadataFromMetrics = excludeMetadataFromMetrics;

        workouts.Update(workoutId, workout);
    }

    public bool RemoveWorkout(Guid workoutId)
    {
        var exercises = GetExercises(workoutId);

        foreach (var exercise in exercises)
            RemoveExercise(exercise.Id);

        return workouts.Remove(workoutId);
    }

    public WorkoutExerciseItemModel AddExerciseFromCatalog(
        Guid workoutId,
        string catalogExerciseId,
        IEnumerable<WorkoutSetModel>? sets = null)
    {
        ValidateWorkoutExists(workoutId);

        var catalogItem = exerciseCatalogService.GetById(catalogExerciseId)
            ?? throw new ServiceValidationException("Catalog exercise does not exist.");

        var exercise = CreateExerciseFromCatalogItem(workoutId, catalogItem);

        return AddExercise(workoutId, exercise, sets);
    }

    public WorkoutExerciseItemModel AddCustomExercise(
        Guid workoutId,
        WorkoutExerciseItemModel exercise,
        IEnumerable<WorkoutSetModel>? sets = null)
    {
        ValidateWorkoutExists(workoutId);
        ArgumentNullException.ThrowIfNull(exercise);

        exercise.WorkoutId = workoutId;

        return AddExercise(workoutId, exercise, sets);
    }

    public void UpdateExercise(
        Guid workoutExerciseId,
        WorkoutExerciseItemModel exercise,
        IEnumerable<WorkoutSetModel> sets)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(sets);

        var existingExercise = workoutExercises.GetById(workoutExerciseId)
            ?? throw new ServiceValidationException("Workout exercise does not exist.");

        exercise.Id = workoutExerciseId;
        exercise.WorkoutId = existingExercise.WorkoutId;

        ValidateWorkoutExercise(exercise);
        ValidateWorkoutSets(sets);

        workoutExercises.Update(workoutExerciseId, exercise);
        ReplaceSets(workoutExerciseId, sets);
    }

    public bool RemoveExercise(Guid workoutExerciseId)
    {
        var sets = GetSets(workoutExerciseId);

        foreach (var set in sets)
            workoutSets.Remove(set.Id);

        return workoutExercises.Remove(workoutExerciseId);
    }

    public void ReorderWorkouts(IReadOnlyList<Guid> orderedWorkoutIds)
    {
        ArgumentNullException.ThrowIfNull(orderedWorkoutIds);

        for (var index = 0; index < orderedWorkoutIds.Count; index++)
        {
            var workout = workouts.GetById(orderedWorkoutIds[index]);

            if (workout is null)
                continue;

            workout.SortNumber = index;
            workouts.Update(workout.Id, workout);
        }
    }

    public void ReorderExercises(Guid workoutId, IReadOnlyList<Guid> orderedExerciseIds)
    {
        ValidateWorkoutExists(workoutId);
        ArgumentNullException.ThrowIfNull(orderedExerciseIds);

        for (var index = 0; index < orderedExerciseIds.Count; index++)
        {
            var exercise = workoutExercises.GetById(orderedExerciseIds[index]);

            if (exercise is null || exercise.WorkoutId != workoutId)
                continue;

            exercise.SortNumber = index;
            workoutExercises.Update(exercise.Id, exercise);
        }
    }

    public void ApplyCompletedActiveSetToTemplate(
        Guid workoutExerciseId,
        int sortNumber,
        int reps,
        double? weightKg,
        int restSeconds)
    {
        var plannedSet = workoutSets
            .Get(query =>
                query.Where(set =>
                    set.WorkoutExerciseId == workoutExerciseId
                    && set.SortNumber == sortNumber))
            .FirstOrDefault();

        if (plannedSet is null)
            return;

        plannedSet.Reps = Math.Max(0, reps);
        plannedSet.WeightKg = weightKg.HasValue ? Math.Max(0, weightKg.Value) : null;
        plannedSet.RestSeconds = Math.Max(0, restSeconds);

        workoutSets.Update(plannedSet.Id, plannedSet);
    }

    private WorkoutExerciseItemModel AddExercise(
        Guid workoutId,
        WorkoutExerciseItemModel exercise,
        IEnumerable<WorkoutSetModel>? sets)
    {
        var materializedSets = sets?.ToList() ?? CreateDefaultSets();

        exercise.WorkoutId = workoutId;
        exercise.SortNumber = GetExercises(workoutId).Count;

        ValidateWorkoutExercise(exercise);
        ValidateWorkoutSets(materializedSets);

        var createdExercise = workoutExercises.Create(exercise);

        foreach (var set in materializedSets.OrderBy(set => set.SortNumber))
        {
            set.WorkoutExerciseId = createdExercise.Id;
            workoutSets.Create(set);
        }

        return createdExercise;
    }

    private void ReplaceSets(Guid workoutExerciseId, IEnumerable<WorkoutSetModel> sets)
    {
        var existingSets = GetSets(workoutExerciseId);

        foreach (var existingSet in existingSets)
            workoutSets.Remove(existingSet.Id);

        var index = 0;

        foreach (var set in sets.OrderBy(set => set.SortNumber))
        {
            set.WorkoutExerciseId = workoutExerciseId;
            set.SortNumber = index++;

            workoutSets.Create(set);
        }
    }

    private static WorkoutExerciseItemModel CreateExerciseFromCatalogItem(
        Guid workoutId,
        ExerciseCatalogItemModel catalogItem)
    {
        return new WorkoutExerciseItemModel
        {
            WorkoutId = workoutId,
            CatalogExerciseId = catalogItem.Id,
            Name = catalogItem.Name,
            Notes = catalogItem.Notes,
            ImageSource = catalogItem.ImageSource,
            Force = catalogItem.Force,
            BodyCategory = catalogItem.BodyCategory,
            Mechanic = catalogItem.Mechanic,
            Equipment = catalogItem.Equipment,
            LimbInvolvement = catalogItem.LimbInvolvement,
            MovementPattern = catalogItem.MovementPattern,
            PrimaryMuscleCategories = [.. catalogItem.PrimaryMuscleCategories],
            SecondaryMuscleCategories = [.. catalogItem.SecondaryMuscleCategories],
            PrimaryMuscles = [.. catalogItem.PrimaryMuscles],
            SecondaryMuscles = [.. catalogItem.SecondaryMuscles]
        };
    }

    private static List<WorkoutSetModel> CreateDefaultSets()
    {
        return
        [
            new WorkoutSetModel
            {
                SortNumber = 0,
                Reps = 10,
                WeightKg = 0,
                RestSeconds = 90
            }
        ];
    }

    private void ValidateWorkoutExists(Guid workoutId)
    {
        if (!Exists(workoutId))
            throw new ServiceValidationException("Workout does not exist.");
    }

    private static void ValidateWorkoutName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ServiceValidationException("Workout name is required.");
    }

    private static void ValidateWorkoutExercise(WorkoutExerciseItemModel exercise)
    {
        if (string.IsNullOrWhiteSpace(exercise.Name))
            throw new ServiceValidationException("Exercise name is required.");

        exercise.Name = exercise.Name.Trim();
        exercise.Notes = exercise.Notes.Trim();
    }

    private static void ValidateWorkoutSets(IEnumerable<WorkoutSetModel> sets)
    {
        var materializedSets = sets.ToList();

        if (materializedSets.Count == 0)
            throw new ServiceValidationException("At least one set is required.");

        foreach (var set in materializedSets)
        {
            if (set.Reps < 0)
                throw new ServiceValidationException("Set reps cannot be negative.");

            if (set.RestSeconds < 0)
                throw new ServiceValidationException("Set rest seconds cannot be negative.");

            if (set.WeightKg < 0)
                throw new ServiceValidationException("Set weight cannot be negative.");
        }
    }
}