using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryRoutineService : IRoutineService
{
    private readonly IExerciseCatalogService _catalogService;
    private readonly List<Routine> _routines;

    public InMemoryRoutineService(IExerciseCatalogService catalogService)
    {
        _catalogService = catalogService;
        _routines = SeedRoutines();
    }

    public Task<IReadOnlyList<Routine>> GetAllAsync()
    {
        return Task.FromResult((IReadOnlyList<Routine>)_routines);
    }

    public Task<Routine> CreateRoutineAsync(string name)
    {
        var routine = new Routine
        {
            Name = name.Trim()
        };

        _routines.Add(routine);
        return Task.FromResult(routine);
    }

    public Task RenameRoutineAsync(Guid routineId, string newName)
    {
        var routine = GetRoutine(routineId);
        routine.Name = newName.Trim();
        return Task.CompletedTask;
    }

    public Task<Routine> DuplicateRoutineAsync(Guid routineId)
    {
        var source = GetRoutine(routineId);

        var copy = new Routine
        {
            Name = source.Name + " Copy",
            Workouts = source.Workouts.Select(CloneWorkout).ToList()
        };

        _routines.Add(copy);
        return Task.FromResult(copy);
    }

    public Task DeleteRoutineAsync(Guid routineId)
    {
        var routine = GetRoutine(routineId);
        _routines.Remove(routine);
        return Task.CompletedTask;
    }

    public Task<WorkoutExercise?> GetExerciseAsync(Guid routineId, Guid workoutId, Guid workoutExerciseId)
    {
        var workout = GetWorkout(routineId, workoutId);
        var exercise = workout.Exercises.FirstOrDefault(x => x.Id == workoutExerciseId);

        return Task.FromResult(exercise is null ? null : CloneExercise(exercise));
    }

    public Task<Workout> AddWorkoutAsync(Guid routineId, string workoutName)
    {
        var routine = GetRoutine(routineId);

        var workout = new Workout
        {
            Name = workoutName.Trim()
        };

        routine.Workouts.Add(workout);
        return Task.FromResult(workout);
    }

    public Task RenameWorkoutAsync(Guid routineId, Guid workoutId, string newName)
    {
        var workout = GetWorkout(routineId, workoutId);
        workout.Name = newName.Trim();
        return Task.CompletedTask;
    }

    public Task<Workout> DuplicateWorkoutAsync(Guid routineId, Guid workoutId)
    {
        var routine = GetRoutine(routineId);
        var source = GetWorkout(routineId, workoutId);

        var copy = CloneWorkout(source);
        copy.Name = source.Name + " Copy";

        routine.Workouts.Add(copy);
        return Task.FromResult(copy);
    }

    public Task DeleteWorkoutAsync(Guid routineId, Guid workoutId)
    {
        var routine = GetRoutine(routineId);
        var workout = GetWorkout(routineId, workoutId);

        routine.Workouts.Remove(workout);
        return Task.CompletedTask;
    }

    public async Task<WorkoutExercise> AddExerciseAsync(Guid routineId, Guid workoutId, string catalogExerciseId)
    {
        var workout = GetWorkout(routineId, workoutId);

        var catalog = await _catalogService.GetByIdAsync(catalogExerciseId)
            ?? throw new InvalidOperationException($"Catalog exercise '{catalogExerciseId}' was not found.");

        var exercise = CreateWorkoutExerciseFromCatalog(catalog, workout.Exercises.Count);
        workout.Exercises.Add(exercise);

        return exercise;
    }

    public Task UpdateExerciseAsync(Guid routineId, Guid workoutId, WorkoutExercise exercise)
    {
        var workout = GetWorkout(routineId, workoutId);
        var existing = workout.Exercises.First(x => x.Id == exercise.Id);

        existing.DefaultRestSeconds = exercise.DefaultRestSeconds;
        existing.Notes = exercise.Notes;
        existing.ImagePath = exercise.ImagePath;
        existing.Sets = exercise.Sets
            .OrderBy(x => x.Order)
            .Select(CloneSet)
            .ToList();

        return Task.CompletedTask;
    }

    public Task DeleteExerciseAsync(Guid routineId, Guid workoutId, Guid workoutExerciseId)
    {
        var workout = GetWorkout(routineId, workoutId);
        var exercise = workout.Exercises.First(x => x.Id == workoutExerciseId);

        workout.Exercises.Remove(exercise);

        for (int i = 0; i < workout.Exercises.Count; i++)
            workout.Exercises[i].SortOrder = i;

        return Task.CompletedTask;
    }

    private List<Routine> SeedRoutines()
    {
        return new List<Routine>
        {
            new Routine
            {
                Name = "Push Pull Legs",
                Workouts = new List<Workout>
                {
                    new Workout
                    {
                        Name = "Push",
                        Exercises = new List<WorkoutExercise>
                        {
                            CreateWorkoutExercise("Bench_Press", 0, new[] { 10, 8, 6 }, new[] { 60.0, 70.0, 80.0 }, 120),
                            CreateWorkoutExercise("Incline_Dumbbell_Press", 1, new[] { 10, 10, 8 }, new[] { 24.0, 24.0, 28.0 }, 90),
                            CreateWorkoutExercise("Arnold_Press", 2, new[] { 12, 10, 10 }, new[] { 14.0, 16.0, 16.0 }, 75),
                            CreateWorkoutExercise("Cable_Lateral_Raise", 3, new[] { 15, 15, 15 }, new[] { 8.0, 8.0, 8.0 }, 60),
                            CreateWorkoutExercise("Triceps_Pushdown", 4, new[] { 12, 12, 10 }, new[] { 25.0, 30.0, 35.0 }, 60)
                        }
                    },
                    new Workout
                    {
                        Name = "Pull",
                        Exercises = new List<WorkoutExercise>
                        {
                            CreateWorkoutExercise("Pull_Up", 0, new[] { 8, 8, 6 }, new[] { 0.0, 0.0, 0.0 }, 120),
                            CreateWorkoutExercise("Lat_Pulldown", 1, new[] { 12, 10, 10 }, new[] { 55.0, 60.0, 65.0 }, 75),
                            CreateWorkoutExercise("Seated_Row", 2, new[] { 12, 10, 10 }, new[] { 55.0, 60.0, 65.0 }, 75),
                            CreateWorkoutExercise("Hammer_Curl", 3, new[] { 12, 10, 10 }, new[] { 14.0, 16.0, 16.0 }, 60),
                            CreateWorkoutExercise("Face_Pull", 4, new[] { 15, 15, 12 }, new[] { 20.0, 20.0, 25.0 }, 60)
                        }
                    },
                    new Workout
                    {
                        Name = "Legs",
                        Exercises = new List<WorkoutExercise>
                        {
                            CreateWorkoutExercise("Leg_Press", 0, new[] { 12, 10, 10 }, new[] { 160.0, 180.0, 180.0 }, 120),
                            CreateWorkoutExercise("Romanian_Deadlift", 1, new[] { 10, 8, 8 }, new[] { 70.0, 80.0, 85.0 }, 120),
                            CreateWorkoutExercise("Leg_Curl", 2, new[] { 12, 12, 10 }, new[] { 40.0, 45.0, 50.0 }, 60),
                            CreateWorkoutExercise("Standing_Calf_Raise", 3, new[] { 15, 15, 12 }, new[] { 60.0, 70.0, 80.0 }, 60),
                            CreateDurationWorkoutExercise("Plank", 4, new[] { 30, 30, 45 }, 45)
                        }
                    }
                }
            },
            new Routine
            {
                Name = "Upper Lower",
                Workouts = new List<Workout>
                {
                    new Workout
                    {
                        Name = "Upper",
                        Exercises = new List<WorkoutExercise>
                        {
                            CreateWorkoutExercise("Incline_Dumbbell_Press", 0, new[] { 10, 10, 8 }, new[] { 22.0, 24.0, 26.0 }, 90),
                            CreateWorkoutExercise("Lat_Pulldown", 1, new[] { 12, 10, 10 }, new[] { 50.0, 55.0, 60.0 }, 75),
                            CreateWorkoutExercise("Arnold_Press", 2, new[] { 12, 10, 10 }, new[] { 12.0, 14.0, 14.0 }, 75),
                            CreateWorkoutExercise("Hammer_Curl", 3, new[] { 12, 10, 10 }, new[] { 12.0, 14.0, 14.0 }, 60)
                        }
                    },
                    new Workout
                    {
                        Name = "Lower",
                        Exercises = new List<WorkoutExercise>
                        {
                            CreateWorkoutExercise("Hack_Squat", 0, new[] { 12, 10, 8 }, new[] { 80.0, 100.0, 110.0 }, 120),
                            CreateWorkoutExercise("Leg_Extension", 1, new[] { 15, 12, 12 }, new[] { 35.0, 40.0, 45.0 }, 60),
                            CreateWorkoutExercise("Leg_Curl", 2, new[] { 15, 12, 12 }, new[] { 35.0, 40.0, 45.0 }, 60),
                            CreateWorkoutExercise("Standing_Calf_Raise", 3, new[] { 15, 15, 15 }, new[] { 50.0, 60.0, 70.0 }, 60)
                        }
                    }
                }
            }
        };
    }

    private WorkoutExercise CreateWorkoutExercise(
        string catalogId,
        int sortOrder,
        IReadOnlyList<int> reps,
        IReadOnlyList<double> weightKg,
        int restSeconds)
    {
        var catalog = _catalogService.GetByIdAsync(catalogId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException($"Catalog exercise '{catalogId}' was not found.");

        var exercise = CreateWorkoutExerciseFromCatalog(catalog, sortOrder);
        exercise.DefaultRestSeconds = restSeconds;
        exercise.Sets = reps.Select((repValue, index) => new WorkoutExerciseSet
        {
            Order = index + 1,
            Reps = repValue,
            WeightKg = weightKg[index],
            DurationSeconds = null,
            RestSeconds = restSeconds
        }).ToList();

        return exercise;
    }

    private WorkoutExercise CreateDurationWorkoutExercise(
        string catalogId,
        int sortOrder,
        IReadOnlyList<int> durationSeconds,
        int restSeconds)
    {
        var catalog = _catalogService.GetByIdAsync(catalogId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException($"Catalog exercise '{catalogId}' was not found.");

        var exercise = CreateWorkoutExerciseFromCatalog(catalog, sortOrder);
        exercise.DefaultRestSeconds = restSeconds;
        exercise.Sets = durationSeconds.Select((seconds, index) => new WorkoutExerciseSet
        {
            Order = index + 1,
            Reps = null,
            WeightKg = 0,
            DurationSeconds = seconds,
            RestSeconds = restSeconds
        }).ToList();

        return exercise;
    }

    private static WorkoutExercise CreateWorkoutExerciseFromCatalog(ExerciseCatalogItem catalog, int sortOrder)
    {
        return new WorkoutExercise
        {
            CatalogExerciseId = catalog.Id,
            SortOrder = sortOrder,
            Name = catalog.Name,
            Force = catalog.Force,
            BodyCategory = catalog.BodyCategory,
            Mechanic = catalog.Mechanic,
            Equipment = catalog.Equipment,
            PrimaryMuscles = catalog.PrimaryMuscles.ToList(),
            SecondaryMuscles = catalog.SecondaryMuscles.ToList(),
            PrimaryMuscleCategories = catalog.PrimaryMuscleCategories.ToList(),
            SecondaryMuscleCategories = catalog.SecondaryMuscleCategories.ToList(),
            LimbInvolvement = catalog.LimbInvolvement,
            MovementPattern = catalog.MovementPattern,
            DefaultRestSeconds = 90,
            Sets = new List<WorkoutExerciseSet>
            {
                new WorkoutExerciseSet { Order = 1, Reps = 10, WeightKg = 0, RestSeconds = 90 },
                new WorkoutExerciseSet { Order = 2, Reps = 10, WeightKg = 0, RestSeconds = 90 },
                new WorkoutExerciseSet { Order = 3, Reps = 10, WeightKg = 0, RestSeconds = 90 }
            }
        };
    }

    private Routine GetRoutine(Guid routineId)
    {
        return _routines.First(x => x.Id == routineId);
    }

    private Workout GetWorkout(Guid routineId, Guid workoutId)
    {
        return GetRoutine(routineId).Workouts.First(x => x.Id == workoutId);
    }

    private static Workout CloneWorkout(Workout source)
    {
        return new Workout
        {
            Name = source.Name,
            Exercises = source.Exercises
                .OrderBy(x => x.SortOrder)
                .Select(CloneExercise)
                .ToList()
        };
    }

    private static WorkoutExercise CloneExercise(WorkoutExercise source)
    {
        return new WorkoutExercise
        {
            CatalogExerciseId = source.CatalogExerciseId,
            SortOrder = source.SortOrder,
            Name = source.Name,
            Force = source.Force,
            BodyCategory = source.BodyCategory,
            Mechanic = source.Mechanic,
            Equipment = source.Equipment,
            PrimaryMuscles = source.PrimaryMuscles.ToList(),
            SecondaryMuscles = source.SecondaryMuscles.ToList(),
            PrimaryMuscleCategories = source.PrimaryMuscleCategories.ToList(),
            SecondaryMuscleCategories = source.SecondaryMuscleCategories.ToList(),
            LimbInvolvement = source.LimbInvolvement,
            MovementPattern = source.MovementPattern,
            DefaultRestSeconds = source.DefaultRestSeconds,
            Notes = source.Notes,
            ImagePath = source.ImagePath,
            Sets = source.Sets
                .OrderBy(x => x.Order)
                .Select(CloneSet)
                .ToList()
        };
    }

    public Task<WorkoutExercise> AddExerciseAsync(Guid routineId, Guid workoutId, WorkoutExercise exercise)
    {
        var workout = GetWorkout(routineId, workoutId);

        exercise.Id = Guid.NewGuid();
        exercise.SortOrder = workout.Exercises.Count;

        for (int i = 0; i < exercise.Sets.Count; i++)
            exercise.Sets[i].Order = i + 1;

        workout.Exercises.Add(CloneExercise(exercise));
        return Task.FromResult(exercise);
    }

    private static WorkoutExerciseSet CloneSet(WorkoutExerciseSet source)
    {
        return new WorkoutExerciseSet
        {
            Order = source.Order,
            Reps = source.Reps,
            WeightKg = source.WeightKg,
            DurationSeconds = source.DurationSeconds,
            RestSeconds = source.RestSeconds,
            Notes = source.Notes
        };
    }
}