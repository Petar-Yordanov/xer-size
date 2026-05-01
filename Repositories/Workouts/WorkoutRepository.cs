using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.Workouts;

public sealed class WorkoutRepository
    : SqliteRepository<WorkoutModel, Guid>
{
    public WorkoutRepository(SqliteLocalStore database)
        : base(database, "Workouts")
    {
    }
}