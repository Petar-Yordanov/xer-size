using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.ActiveWorkout;

public sealed class ActiveWorkoutSessionRepository
    : SqliteRepository<ActiveWorkoutSessionModel, Guid>
{
    public ActiveWorkoutSessionRepository(SqliteLocalStore database)
        : base(database, "ActiveWorkoutSessions")
    {
    }
}