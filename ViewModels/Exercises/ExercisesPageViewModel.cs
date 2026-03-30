using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.Exercises;

public partial class ExercisesPageViewModel : ObservableObject
{
    private readonly IExerciseCatalogService _catalogService;
    private List<ExerciseCatalogItem> _allItems = new();
    private CancellationTokenSource? _filterCts;
    private bool _isLoaded;

    public ObservableCollection<ExercisesCatalogCardRow> Items { get; } = new();

    public ObservableCollection<ExercisesCatalogOptionRow> ForceOptions { get; } = new();
    public ObservableCollection<ExercisesCatalogOptionRow> EquipmentOptions { get; } = new();
    public ObservableCollection<ExercisesCatalogOptionRow> BodyCategoryOptions { get; } = new();

    [ObservableProperty] public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty] public partial string SelectedForce { get; set; } = "All";
    [ObservableProperty] public partial string SelectedEquipment { get; set; } = "All";
    [ObservableProperty] public partial string SelectedBodyCategory { get; set; } = "All";

    [ObservableProperty] public partial bool IsForceSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsEquipmentSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsBodyCategorySheetOpen { get; set; }

    public string SelectedForceSummary => BuildSingleSummary(SelectedForce, "Force");
    public string SelectedEquipmentSummary => BuildSingleSummary(SelectedEquipment, "Equipment");
    public string SelectedBodyCategorySummary => BuildSingleSummary(SelectedBodyCategory, "Body Category");

    public ExercisesPageViewModel(IExerciseCatalogService catalogService)
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

        SelectedForce = "All";
        SelectedEquipment = "All";
        SelectedBodyCategory = "All";

        ApplyFiltersNow();
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task EditAsync(ExercisesCatalogCardRow? row)
    {
        if (row?.Item is null)
            return;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        await page.DisplayAlertAsync("Edit Catalog Exercise", $"Editing '{row.Item.Name}' is not implemented yet.", "OK");
    }

    [RelayCommand]
    private void ToggleExpanded(ExercisesCatalogCardRow? row)
    {
        if (row is null)
            return;

        row.IsExpanded = !row.IsExpanded;
    }

    [RelayCommand]
    private void ToggleForceSheet()
    {
        CloseAllSheets();
        IsForceSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleEquipmentSheet()
    {
        CloseAllSheets();
        IsEquipmentSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleBodyCategorySheet()
    {
        CloseAllSheets();
        IsBodyCategorySheetOpen = true;
    }

    [RelayCommand] private void CloseForceSheet() => IsForceSheetOpen = false;
    [RelayCommand] private void CloseEquipmentSheet() => IsEquipmentSheetOpen = false;
    [RelayCommand] private void CloseBodyCategorySheet() => IsBodyCategorySheetOpen = false;

    [RelayCommand]
    private void SelectForce(ExercisesCatalogOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        SelectedForce = option.Text;
        IsForceSheetOpen = false;
    }

    [RelayCommand]
    private void SelectEquipment(ExercisesCatalogOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        SelectedEquipment = option.Text;
        IsEquipmentSheetOpen = false;
    }

    [RelayCommand]
    private void SelectBodyCategory(ExercisesCatalogOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        SelectedBodyCategory = option.Text;
        IsBodyCategorySheetOpen = false;
    }

    partial void OnSearchTextChanged(string value) => _ = DebouncedApplyFiltersAsync();

    partial void OnSelectedForceChanged(string value)
    {
        UpdateSelectedSingleOptions(ForceOptions, value);
        OnPropertyChanged(nameof(SelectedForceSummary));
        ApplyFiltersNow();
    }

    partial void OnSelectedEquipmentChanged(string value)
    {
        UpdateSelectedSingleOptions(EquipmentOptions, value);
        OnPropertyChanged(nameof(SelectedEquipmentSummary));
        ApplyFiltersNow();
    }

    partial void OnSelectedBodyCategoryChanged(string value)
    {
        UpdateSelectedSingleOptions(BodyCategoryOptions, value);
        OnPropertyChanged(nameof(SelectedBodyCategorySummary));
        ApplyFiltersNow();
    }

    private void RebuildFilters()
    {
        RebuildFilterCollection(ForceOptions, _allItems.Select(x => x.Force));
        RebuildFilterCollection(EquipmentOptions, _allItems.Select(x => x.Equipment));
        RebuildFilterCollection(BodyCategoryOptions, _allItems.Select(x => x.BodyCategory));

        UpdateSelectedSingleOptions(ForceOptions, SelectedForce);
        UpdateSelectedSingleOptions(EquipmentOptions, SelectedEquipment);
        UpdateSelectedSingleOptions(BodyCategoryOptions, SelectedBodyCategory);
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
        var search = SearchText?.Trim();
        var force = string.IsNullOrWhiteSpace(SelectedForce) ? "All" : SelectedForce;
        var equipment = string.IsNullOrWhiteSpace(SelectedEquipment) ? "All" : SelectedEquipment;
        var bodyCategory = string.IsNullOrWhiteSpace(SelectedBodyCategory) ? "All" : SelectedBodyCategory;

        var filtered = _allItems
            .Where(x => string.IsNullOrWhiteSpace(search) || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Where(x => force == "All" || string.Equals(x.Force, force, StringComparison.OrdinalIgnoreCase))
            .Where(x => equipment == "All" || string.Equals(x.Equipment, equipment, StringComparison.OrdinalIgnoreCase))
            .Where(x => bodyCategory == "All" || string.Equals(x.BodyCategory, bodyCategory, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new ExercisesCatalogCardRow(x))
            .ToList();

        Items.Clear();
        foreach (var item in filtered)
            Items.Add(item);
    }

    private static void RebuildFilterCollection(ObservableCollection<ExercisesCatalogOptionRow> target, IEnumerable<string> values)
    {
        target.Clear();
        target.Add(new ExercisesCatalogOptionRow { Text = "All", IsSelected = true });

        foreach (var value in values
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            target.Add(new ExercisesCatalogOptionRow { Text = value });
        }
    }

    private static void UpdateSelectedSingleOptions(IEnumerable<ExercisesCatalogOptionRow> target, string? selectedValue)
    {
        foreach (var item in target)
        {
            item.IsSelected = !string.IsNullOrWhiteSpace(selectedValue)
                && string.Equals(item.Text, selectedValue, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string BuildSingleSummary(string? value, string emptyText)
    {
        return string.IsNullOrWhiteSpace(value) ? emptyText : value;
    }

    private void CloseAllSheets()
    {
        IsForceSheetOpen = false;
        IsEquipmentSheetOpen = false;
        IsBodyCategorySheetOpen = false;
    }
}

public partial class ExercisesCatalogOptionRow : ObservableObject
{
    [ObservableProperty] public partial string Text { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSelected { get; set; }
}

public partial class ExercisesCatalogCardRow : ObservableObject
{
    public ExerciseCatalogItem Item { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public ExercisesCatalogCardRow(ExerciseCatalogItem item)
    {
        Item = item;
    }

    public string Name => Item.Name;
    public string Equipment => Item.Equipment;
    public string Mechanic => Item.Mechanic;
    public string Force => Item.Force;
    public string BodyCategory => Item.BodyCategory;
    public string LimbInvolvement => Item.LimbInvolvement;
    public string MovementPattern => string.IsNullOrWhiteSpace(Item.MovementPattern) ? "Not specified" : Item.MovementPattern!;

    public string PrimaryMusclesText => BuildList(Item.PrimaryMuscleCategories);
    public string SecondaryMusclesText => BuildList(Item.SecondaryMuscleCategories);
    public string ExpandButtonText => IsExpanded ? "Hide Info" : "Expand Info";

    partial void OnIsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ExpandButtonText));
    }

    private static string BuildList(IEnumerable<string> items)
    {
        var values = items
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return values.Count == 0 ? "None" : string.Join(", ", values);
    }
}