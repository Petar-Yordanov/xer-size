using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.ActiveWorkout;

public sealed class ActiveWorkoutExerciseRepository
    : SqliteRepository<ActiveWorkoutExerciseModel, Guid>
{
    public ActiveWorkoutExerciseRepository(SqliteLocalStore database)
        : base(database, "ActiveWorkoutExercises")
    {
    }
}