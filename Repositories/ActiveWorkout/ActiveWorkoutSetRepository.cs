using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.ActiveWorkout;

public sealed class ActiveWorkoutSetRepository
    : SqliteRepository<ActiveWorkoutSetModel, Guid>
{
    public ActiveWorkoutSetRepository(SqliteLocalStore database)
        : base(database, "ActiveWorkoutSets")
    {
    }
}