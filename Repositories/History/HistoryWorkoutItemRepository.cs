using XerSize.Models.DataAccessObjects.History;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.History;

public sealed class HistoryWorkoutItemRepository
    : SqliteRepository<HistoryWorkoutItemModel, Guid>
{
    public HistoryWorkoutItemRepository(SqliteLocalStore database)
        : base(database, "HistoryWorkouts")
    {
    }
}