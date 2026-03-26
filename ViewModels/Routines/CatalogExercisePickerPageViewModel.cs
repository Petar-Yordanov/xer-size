using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.Views.Pages;

namespace XerSize.ViewModels.Routines;

[QueryProperty(nameof(RoutineIdText), "routineId")]
[QueryProperty(nameof(WorkoutIdText), "workoutId")]
public partial class CatalogExercisePickerPageViewModel : ObservableObject
{
    private readonly IExerciseCatalogService _catalogService;
    private List<ExerciseCatalogItem> _allItems = new();
    private CancellationTokenSource? _filterCts;
    private bool _isLoaded;

    public ObservableCollection<ExerciseCatalogItem> Items { get; } = new();

    public ObservableCollection<string> ForceOptions { get; } = new() { "All" };
    public ObservableCollection<string> EquipmentOptions { get; } = new() { "All" };
    public ObservableCollection<string> BodyCategoryOptions { get; } = new() { "All" };

    [ObservableProperty] public partial string RoutineIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WorkoutIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SearchText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SelectedForce { get; set; } = "All";
    [ObservableProperty] public partial string SelectedEquipment { get; set; } = "All";
    [ObservableProperty] public partial string SelectedBodyCategory { get; set; } = "All";

    public CatalogExercisePickerPageViewModel(IExerciseCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        _allItems = (await _catalogService.GetAllAsync())
            .OrderBy(x => x.Name)
            .ToList();

        RebuildFilters();
        ApplyFiltersNow();

        _isLoaded = true;
    }

    [RelayCommand]
    private Task PickAsync(ExerciseCatalogItem? item)
    {
        if (item is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync(
            $"{nameof(AddExercisePage)}?routineId={RoutineIdText}&workoutId={WorkoutIdText}&catalogExerciseId={item.Id}");
    }

    partial void OnSearchTextChanged(string value) => _ = DebouncedApplyFiltersAsync();
    partial void OnSelectedForceChanged(string value) => ApplyFiltersNow();
    partial void OnSelectedEquipmentChanged(string value) => ApplyFiltersNow();
    partial void OnSelectedBodyCategoryChanged(string value) => ApplyFiltersNow();

    private void RebuildFilters()
    {
        RebuildFilterCollection(ForceOptions, _allItems.Select(x => x.Force));
        RebuildFilterCollection(EquipmentOptions, _allItems.Select(x => x.Equipment));
        RebuildFilterCollection(BodyCategoryOptions, _allItems.Select(x => x.BodyCategory));
    }

    private async Task DebouncedApplyFiltersAsync()
    {
        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();
        var token = _filterCts.Token;

        try
        {
            await Task.Delay(180, token);
            if (!token.IsCancellationRequested)
                ApplyFiltersNow();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void ApplyFiltersNow()
    {
        var search = SearchText;
        var force = SelectedForce;
        var equipment = SelectedEquipment;
        var bodyCategory = SelectedBodyCategory;

        var filtered = _allItems
            .Where(x => string.IsNullOrWhiteSpace(search) || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Where(x => force == "All" || x.Force == force)
            .Where(x => equipment == "All" || x.Equipment == equipment)
            .Where(x => bodyCategory == "All" || x.BodyCategory == bodyCategory)
            .OrderBy(x => x.Name)
            .ToList();

        Items.Clear();
        foreach (var item in filtered)
            Items.Add(item);
    }

    private static void RebuildFilterCollection(ObservableCollection<string> target, IEnumerable<string> values)
    {
        target.Clear();
        target.Add("All");

        foreach (var value in values
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            target.Add(value);
        }
    }
}