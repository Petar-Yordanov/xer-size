using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.Workouts;

public sealed class WorkoutSetRepository
    : SqliteRepository<WorkoutSetModel, Guid>
{
    public WorkoutSetRepository(SqliteLocalStore database)
        : base(database, "WorkoutSets")
    {
    }
}