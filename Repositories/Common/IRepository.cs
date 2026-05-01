namespace XerSize.Repositories.Common;

public interface IRepository<TModel, TId>
    where TId : notnull
{
    TModel Create(TModel model);

    bool Remove(TId id);

    bool Update(TId id, TModel model);

    TModel? GetById(TId id);

    IReadOnlyList<TModel> Get(
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null);

    PagedResult<TModel> GetPage(
        int pageNumber = 1,
        int pageSize = 50,
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null);

    int Count(
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null);

    void Clear();
}