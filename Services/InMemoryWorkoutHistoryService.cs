using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryWorkoutHistoryService : IWorkoutHistoryService
{
    private readonly List<LoggedWorkoutSession> _sessions;

    public InMemoryWorkoutHistoryService(IRoutineService routineService)
    {
        var routines = routineService.GetAllAsync().GetAwaiter().GetResult();
        _sessions = GenerateSessions(routines);
    }

    public Task<IReadOnlyList<LoggedWorkoutSession>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<LoggedWorkoutSession>)_sessions);

    private static List<LoggedWorkoutSession> GenerateSessions(IReadOnlyList<Routine> routines)
    {
        var sessions = new List<LoggedWorkoutSession>();
        var random = new Random(42);
        var startDate = DateTime.Today.AddMonths(-7);
        var allWorkouts = routines.SelectMany(r => r.Workouts.Select(w => (Routine: r, Workout: w))).ToList();

        int workoutIndex = 0;
        int sessionIndex = 0;

        for (var date = startDate; date <= DateTime.Today; date = date.AddDays(2))
        {
            if (date.DayOfWeek == DayOfWeek.Sunday && random.NextDouble() < 0.55)
                continue;

            var selected = allWorkouts[workoutIndex % allWorkouts.Count];
            workoutIndex++;
            sessionIndex++;

            var bodyWeight = 84.0 + Math.Sin(sessionIndex / 8.0) * 1.1 - sessionIndex * 0.01;
            var loggedExercises = new List<LoggedWorkoutExercise>();

            foreach (var template in selected.Workout.Exercises.OrderBy(x => x.SortOrder))
            {
                loggedExercises.Add(CreateLoggedExercise(template, sessionIndex, random));
            }

            var duration = 48 + loggedExercises.Count * 6 + random.Next(0, 9);
            var session = new LoggedWorkoutSession
            {
                RoutineId = selected.Routine.Id,
                RoutineName = selected.Routine.Name,
                WorkoutId = selected.Workout.Id,
                WorkoutName = selected.Workout.Name,
                PerformedAt = date.AddHours(18).AddMinutes(random.Next(0, 45)),
                DurationMinutes = duration,
                EstimatedCaloriesBurned = 210 + duration * 4.6,
                BodyWeightKg = Math.Round(bodyWeight, 1),
                Exercises = loggedExercises
            };

            sessions.Add(session);
        }

        return sessions.OrderBy(x => x.PerformedAt).ToList();
    }

    private static LoggedWorkoutExercise CreateLoggedExercise(WorkoutExercise template, int sessionIndex, Random random)
    {
        var progression = sessionIndex * 0.35;
        var logged = new LoggedWorkoutExercise
        {
            WorkoutExerciseId = template.Id,
            CatalogExerciseId = template.CatalogExerciseId,
            Name = template.Name,
            Force = template.Force,
            BodyCategory = template.BodyCategory,
            Mechanic = template.Mechanic,
            Equipment = template.Equipment,
            PrimaryMuscles = template.PrimaryMuscles.ToList(),
            SecondaryMuscles = template.SecondaryMuscles.ToList(),
            PrimaryMuscleCategories = template.PrimaryMuscleCategories.ToList(),
            SecondaryMuscleCategories = template.SecondaryMuscleCategories.ToList(),
            LimbInvolvement = template.LimbInvolvement,
            MovementPattern = template.MovementPattern,
            Sets = new List<LoggedSet>()
        };

        foreach (var set in template.Sets.OrderBy(x => x.Order))
        {
            var isDuration = set.DurationSeconds.HasValue && set.DurationSeconds > 0;
            var baseWeight = set.WeightKg ?? 0;
            var adjustedWeight = baseWeight > 0
                ? Math.Max(0, Math.Round(baseWeight + progression + random.NextDouble() * 2 - 0.8, 1))
                : 0;

            var reps = set.Reps.HasValue
                ? Math.Max(1, set.Reps.Value + random.Next(-1, 2))
                : (int?)null;

            var duration = isDuration
                ? Math.Max(10, (set.DurationSeconds ?? 0) + random.Next(-5, 6))
                : (int?)null;

            logged.Sets.Add(new LoggedSet
            {
                Order = set.Order,
                Reps = reps,
                WeightKg = adjustedWeight,
                DurationSeconds = duration,
                RestSeconds = set.RestSeconds
            });
        }

        return logged;
    }
}