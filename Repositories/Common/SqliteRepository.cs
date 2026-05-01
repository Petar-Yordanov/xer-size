using System.Reflection;
using System.Text.Json;

namespace XerSize.Repositories.Common;

public class SqliteRepository<TModel, TId> : IRepository<TModel, TId>
    where TId : notnull
{
    private static readonly PropertyInfo IdProperty = ResolveIdProperty();

    private readonly SqliteLocalStore database;
    private readonly string tableName;
    private readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
    {
        WriteIndented = false
    };

    public SqliteRepository(SqliteLocalStore database, string tableName)
    {
        this.database = database;
        this.tableName = NormalizeTableName(tableName);

        EnsureTable();
    }

    public virtual TModel Create(TModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (database.SyncRoot)
        {
            var id = GetId(model);

            if (IsDefaultId(id))
            {
                id = CreateNewId();
                SetId(model, id);
            }

            if (ExistsInternal(id))
                throw new InvalidOperationException($"{typeof(TModel).Name} with id '{id}' already exists.");

            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                INSERT INTO {tableName} (Id, JsonData)
                VALUES ($id, $json);
                """;

            command.Parameters.AddWithValue("$id", ToStorageId(id));
            command.Parameters.AddWithValue("$json", Serialize(model));

            command.ExecuteNonQuery();

            return model;
        }
    }

    public virtual bool Remove(TId id)
    {
        lock (database.SyncRoot)
        {
            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                DELETE FROM {tableName}
                WHERE Id = $id;
                """;

            command.Parameters.AddWithValue("$id", ToStorageId(id));

            return command.ExecuteNonQuery() > 0;
        }
    }

    public virtual bool Update(TId id, TModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (database.SyncRoot)
        {
            SetId(model, id);

            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                UPDATE {tableName}
                SET JsonData = $json
                WHERE Id = $id;
                """;

            command.Parameters.AddWithValue("$id", ToStorageId(id));
            command.Parameters.AddWithValue("$json", Serialize(model));

            return command.ExecuteNonQuery() > 0;
        }
    }

    public virtual TModel? GetById(TId id)
    {
        lock (database.SyncRoot)
        {
            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT JsonData
                FROM {tableName}
                WHERE Id = $id
                LIMIT 1;
                """;

            command.Parameters.AddWithValue("$id", ToStorageId(id));

            var json = command.ExecuteScalar() as string;

            return string.IsNullOrWhiteSpace(json)
                ? default
                : Deserialize(json);
        }
    }

    public virtual IReadOnlyList<TModel> Get(
        Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
    {
        lock (database.SyncRoot)
        {
            var source = ReadAllInternal().AsQueryable();
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

        lock (database.SyncRoot)
        {
            var source = ReadAllInternal().AsQueryable();
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
        lock (database.SyncRoot)
        {
            var source = ReadAllInternal().AsQueryable();
            var result = query is null ? source : query(source);

            return result.Count();
        }
    }

    public virtual void Clear()
    {
        lock (database.SyncRoot)
        {
            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM {tableName};";

            command.ExecuteNonQuery();
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

    private void EnsureTable()
    {
        lock (database.SyncRoot)
        {
            using var connection = database.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS {tableName}
                (
                    Id TEXT NOT NULL PRIMARY KEY,
                    JsonData TEXT NOT NULL,
                    CreatedAtUtc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAtUtc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                """;

            command.ExecuteNonQuery();
        }
    }

    private bool ExistsInternal(TId id)
    {
        using var connection = database.OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT 1
            FROM {tableName}
            WHERE Id = $id
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$id", ToStorageId(id));

        return command.ExecuteScalar() is not null;
    }

    private IReadOnlyList<TModel> ReadAllInternal()
    {
        using var connection = database.OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT JsonData
            FROM {tableName};
            """;

        using var reader = command.ExecuteReader();

        var results = new List<TModel>();

        while (reader.Read())
        {
            var json = reader.GetString(0);
            var model = Deserialize(json);

            if (model is not null)
                results.Add(model);
        }

        return results;
    }

    private string Serialize(TModel model)
    {
        return JsonSerializer.Serialize(model, serializerOptions);
    }

    private TModel? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<TModel>(json, serializerOptions);
    }

    private static string ToStorageId(TId id)
    {
        return id switch
        {
            Guid guid => guid.ToString("D"),
            string text => text,
            _ => id.ToString() ?? string.Empty
        };
    }

    private static string NormalizeTableName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SQLite table name cannot be empty.", nameof(value));

        var normalized = new string(value.Where(character =>
            char.IsLetterOrDigit(character) ||
            character == '_').ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("SQLite table name contains no valid characters.", nameof(value));

        return normalized;
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