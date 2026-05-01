using XerSize.Models.DataAccessObjects.Catalog;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.Catalog;

public sealed class ExerciseCatalogItemRepository
    : SqliteRepository<ExerciseCatalogItemModel, string>
{
    public ExerciseCatalogItemRepository(SqliteLocalStore database)
        : base(database, "ExerciseCatalogItems")
    {
    }

    public override ExerciseCatalogItemModel Create(ExerciseCatalogItemModel model)
    {
        throw new NotSupportedException("Exercise catalog items are read-only.");
    }

    public override bool Remove(string id)
    {
        throw new NotSupportedException("Exercise catalog items are read-only.");
    }

    public override bool Update(string id, ExerciseCatalogItemModel model)
    {
        throw new NotSupportedException("Exercise catalog items are read-only.");
    }

    public void LoadCatalogItems(IEnumerable<ExerciseCatalogItemModel> catalogItems)
    {
        ArgumentNullException.ThrowIfNull(catalogItems);

        Clear();

        foreach (var catalogItem in catalogItems)
            base.Create(catalogItem);
    }
}