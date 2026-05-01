using System.Reflection;

namespace XerSize.Repositories.Common;

public class InMemoryRepository<TModel, TId> : IRepository<TModel, TId>
    where TId : notnull
{
    private static readonly PropertyInfo IdProperty = ResolveIdProperty();

    private readonly List<TModel> items = [];
    private readonly object syncRoot = new();

    public virtual TModel Create(TModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (syncRoot)
        {
            var id = GetId(model);

            if (IsDefaultId(id))
            {
                id = CreateNewId();
                SetId(model, id);
            }

            if (items.Any(item => EqualityComparer<TId>.Default.Equals(GetId(item), id)))
                throw new InvalidOperationException($"{typeof(TModel).Name} with id '{id}' already exists.");

            items.Add(model);

            return model;
        }
    }

    public virtual bool Remove(TId id)
    {
        lock (syncRoot)
        {
            var index = items.FindIndex(item => EqualityComparer<TId>.Default.Equals(GetId(item), id));

            if (index < 0)
                return false;

            items.RemoveAt(index);
            return true;
        }
    }

    public virtual bool Update(TId id, TModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (syncRoot)
        {
            var index = items.FindIndex(item => EqualityComparer<TId>.Default.Equals(GetId(item), id));

            if (index < 0)
                return false;

            SetId(model, id);
            items[index] = model;

            return true;
        }
    }

    public virtual TModel? GetById(TId id)
    {
        lock (syncRoot)
        {
            return items.FirstOrDefault(item => EqualityComparer<TId>.Default.Equals(GetId(item), id));
        }
    }

    public virtual IReadOnlyList<TModel> Get(
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
    {
        lock (syncRoot)
        {
            var source = items.ToList().AsQueryable();
            var result = query is null ? source : query(source);

            return result.ToList();
        }
    }

    public virtual PagedResult<TModel> GetPage(
        int pageNumber = 1,
        int pageSize = 50,
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Max(1, pageSize);

        lock (syncRoot)
        {
            var source = items.ToList().AsQueryable();
            var filtered = query is null ? source : query(source);

            var totalCount = filtered.Count();

            var pageItems = filtered
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

            return new PagedResult<TModel>
            {
                Items = pageItems,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize,
                TotalCount = totalCount
            };
        }
    }

    public virtual int Count(
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
    {
        lock (syncRoot)
        {
            var source = items.ToList().AsQueryable();
            var result = query is null ? source : query(source);

            return result.Count();
        }
    }

    public virtual void Clear()
    {
        lock (syncRoot)
        {
            items.Clear();
        }
    }

    protected virtual TId GetId(TModel model)
    {
        var value = IdProperty.GetValue(model);

        if (value is TId typedValue)
            return typedValue;

        throw new InvalidOperationException(
            $"{typeof(TModel).Name}.Id must be assignable to {typeof(TId).Name}.");
    }

    protected virtual void SetId(TModel model, TId id)
    {
        if (!IdProperty.CanWrite)
            throw new InvalidOperationException($"{typeof(TModel).Name}.Id must have a setter.");

        IdProperty.SetValue(model, id);
    }

    protected virtual bool IsDefaultId(TId id)
    {
        if (typeof(TId) == typeof(Guid))
            return EqualityComparer<TId>.Default.Equals(id, (TId)(object)Guid.Empty);

        if (typeof(TId) == typeof(string))
            return string.IsNullOrWhiteSpace(id as string);

        return EqualityComparer<TId>.Default.Equals(id, default!);
    }

    protected virtual TId CreateNewId()
    {
        if (typeof(TId) == typeof(Guid))
            return (TId)(object)Guid.NewGuid();

        if (typeof(TId) == typeof(string))
            throw new InvalidOperationException(
                $"{typeof(TModel).Name}.Id is a string. Create the id before calling Create().");

        throw new InvalidOperationException(
            $"Automatic id creation is not supported for id type {typeof(TId).Name}.");
    }

    private static PropertyInfo ResolveIdProperty()
    {
        var property = typeof(TModel).GetProperty(
            "Id",
            BindingFlags.Instance | BindingFlags.Public);

        if (property is null)
            throw new InvalidOperationException($"{typeof(TModel).Name} must have a public Id property.");

        var propertyType = property.PropertyType;

        if (propertyType != typeof(TId))
        {
            throw new InvalidOperationException(
                $"{typeof(TModel).Name}.Id must be {typeof(TId).Name}, but it is {propertyType.Name}.");
        }

        return property;
    }
}