using XerSize.Models.DataAccessObjects.History;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.History;

public sealed class HistorySetItemRepository
    : SqliteRepository<HistorySetItemModel, Guid>
{
    public HistorySetItemRepository(SqliteLocalStore database)
        : base(database, "HistorySets")
    {
    }
}