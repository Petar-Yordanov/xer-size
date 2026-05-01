using XerSize.Models.DataAccessObjects.History;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.History;

public sealed class HistoryExerciseItemRepository
    : SqliteRepository<HistoryExerciseItemModel, Guid>
{
    public HistoryExerciseItemRepository(SqliteLocalStore database)
        : base(database, "HistoryExercises")
    {
    }
}