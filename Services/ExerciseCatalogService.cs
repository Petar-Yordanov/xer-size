using System.Collections.ObjectModel;
using System.Text.Json;
using XerSize.Models.DataAccessObjects.Catalog;
using XerSize.Models.Serialization;

namespace XerSize.Services;

public sealed class ExerciseCatalogService
{
    private const string CatalogResourcePathMarker = ".ExerciseCatalog.";

    private readonly SemaphoreSlim loadLock = new(1, 1);

    private bool hasLoaded;

    public ObservableCollection<ExerciseCatalogItemModel> Items { get; } = new();

    public IReadOnlyList<ExerciseCatalogItemModel> All => Items;

    public ExerciseCatalogItemModel? PendingSelectedExercise { get; private set; }

    public string LastLoadError { get; private set; } = string.Empty;

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LastLoadError);

    public async Task InitializeAsync()
    {
        if (hasLoaded)
            return;

        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        await loadLock.WaitAsync();

        try
        {
            if (hasLoaded)
                return;

            LastLoadError = string.Empty;
            Items.Clear();

            var assembly = typeof(ExerciseCatalogService).Assembly;

            var resourceNames = assembly
                .GetManifestResourceNames()
                .Where(name =>
                    name.Contains(CatalogResourcePathMarker, StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (resourceNames.Count == 0)
            {
                LastLoadError =
                    "No embedded exercise catalog JSON files were found. Make sure the files are marked as EmbeddedResource and are inside a folder named ExerciseCatalog.";

                hasLoaded = true;
                return;
            }

            var loadedItems = new Dictionary<string, ExerciseCatalogItemModel>(StringComparer.OrdinalIgnoreCase);

            foreach (var resourceName in resourceNames)
            {
                await using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream is null)
                    continue;

                var fileItems = await ReadCatalogItemsAsync(stream);

                foreach (var item in fileItems)
                {
                    Normalize(item);

                    if (string.IsNullOrWhiteSpace(item.Id))
                        continue;

                    loadedItems[item.Id] = item;
                }
            }

            foreach (var item in loadedItems.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
                Items.Add(item);

            if (Items.Count == 0)
            {
                LastLoadError =
                    "Exercise catalog JSON files were found, but no valid exercises were loaded. Check that each item has at least Id and Name.";
            }

            hasLoaded = true;
        }
        catch (Exception ex)
        {
            LastLoadError = $"Failed to load exercise catalog: {ex.Message}";
            hasLoaded = true;
        }
        finally
        {
            loadLock.Release();
        }
    }

    public ExerciseCatalogItemModel? GetById(string? id)
    {
        return FindById(id);
    }

    public ExerciseCatalogItemModel? FindById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return Items.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<ExerciseCatalogItemModel> GetAll()
    {
        return Items;
    }

    public void SetPendingSelectedExercise(ExerciseCatalogItemModel? exercise)
    {
        PendingSelectedExercise = exercise;
    }

    public ExerciseCatalogItemModel? ConsumePendingSelectedExercise()
    {
        var selected = PendingSelectedExercise;
        PendingSelectedExercise = null;
        return selected;
    }

    public void ResetForReload()
    {
        hasLoaded = false;
        LastLoadError = string.Empty;
        PendingSelectedExercise = null;
        Items.Clear();
    }

    private static async Task<IReadOnlyList<ExerciseCatalogItemModel>> ReadCatalogItemsAsync(Stream stream)
    {
        using var document = await JsonDocument.ParseAsync(stream);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            var items = new List<ExerciseCatalogItemModel>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                var item = element.Deserialize<ExerciseCatalogItemModel>(
                    XerSizeJsonSerializerOptions.Default);

                if (item is not null)
                    items.Add(item);
            }

            return items;
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            var item = document.RootElement.Deserialize<ExerciseCatalogItemModel>(
                XerSizeJsonSerializerOptions.Default);

            return item is null
                ? []
                : [item];
        }

        return [];
    }

    private static void Normalize(ExerciseCatalogItemModel item)
    {
        item.Id = item.Id?.Trim() ?? string.Empty;

        item.Name = string.IsNullOrWhiteSpace(item.Name)
            ? item.Id
            : item.Name.Trim();

        item.Notes = item.Notes?.Trim() ?? string.Empty;

        item.ImageSource = string.IsNullOrWhiteSpace(item.ImageSource)
            ? "image.png"
            : item.ImageSource.Trim();

        item.PrimaryMuscleCategories = CleanList(item.PrimaryMuscleCategories);
        item.SecondaryMuscleCategories = CleanList(item.SecondaryMuscleCategories);
        item.PrimaryMuscles = CleanList(item.PrimaryMuscles);
        item.SecondaryMuscles = CleanList(item.SecondaryMuscles);
        item.Aliases = CleanList(item.Aliases);
    }

    private static List<string> CleanList(IEnumerable<string>? values)
    {
        return values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }
}