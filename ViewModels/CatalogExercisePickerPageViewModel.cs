using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models.DataAccessObjects.Catalog;
using XerSize.Models.Presentation.Catalog;
using XerSize.Models.Presentation.ExerciseMetadata;
using XerSize.Models.Presentation.Options;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class CatalogExercisePickerPageViewModel : ObservableObject
{
    private const int PageSize = 80;

    private readonly ExerciseCatalogService exerciseCatalogService;

    private readonly List<ExerciseCatalogItemPresentationModel> allItems = [];
    private readonly List<ExerciseCatalogItemPresentationModel> currentFilteredItems = [];

    private CancellationTokenSource? searchDebounceCts;
    private int loadedCount;
    private bool hasInitialized;

    public CatalogExercisePickerPageViewModel(ExerciseCatalogService exerciseCatalogService)
    {
        this.exerciseCatalogService = exerciseCatalogService;
    }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedForce { get; set; } = ExercisePresentationOptions.AllFilterLabel;

    [ObservableProperty]
    public partial string SelectedBodyCategory { get; set; } = ExercisePresentationOptions.AllFilterLabel;

    [ObservableProperty]
    public partial string SelectedMechanic { get; set; } = ExercisePresentationOptions.AllFilterLabel;

    [ObservableProperty]
    public partial string SelectedEquipment { get; set; } = ExercisePresentationOptions.AllFilterLabel;

    [ObservableProperty]
    public partial string SelectedLimbInvolvement { get; set; } = ExercisePresentationOptions.AllFilterLabel;

    [ObservableProperty]
    public partial bool IsForceExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsBodyCategoryExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsMechanicExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsEquipmentExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsLimbInvolvementExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool CanLoadMore { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCatalogLoadError))]
    [NotifyPropertyChangedFor(nameof(HasNoCatalogLoadError))]
    public partial string CatalogLoadErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResultCountText { get; set; } = "0 exercises";

    public bool HasCatalogLoadError => !string.IsNullOrWhiteSpace(CatalogLoadErrorMessage);

    public bool HasNoCatalogLoadError => !HasCatalogLoadError;

    public ObservableCollection<ExerciseCatalogItemPresentationModel> FilteredExercises { get; } = [];

    public ObservableCollection<string> ForceOptions { get; } = [];

    public ObservableCollection<string> BodyCategoryOptions { get; } = [];

    public ObservableCollection<string> MechanicOptions { get; } = [];

    public ObservableCollection<string> EquipmentOptions { get; } = [];

    public ObservableCollection<string> LimbInvolvementOptions { get; } = [];

    public async Task InitializeAsync()
    {
        if (hasInitialized)
            return;

        hasInitialized = true;
        IsLoading = true;
        ResultCountText = "Loading exercises";

        try
        {
            await exerciseCatalogService.InitializeAsync();

            CatalogLoadErrorMessage = exerciseCatalogService.LastLoadError;

            allItems.Clear();
            allItems.AddRange(exerciseCatalogService.GetAll().Select(ToPresentationModel));

            RebuildOptions();
            ApplyFilters(resetLoadedItems: true);
        }
        catch (Exception ex)
        {
            CatalogLoadErrorMessage = ex.Message;
            ResultCountText = "0 exercises";
            FilteredExercises.Clear();
            CanLoadMore = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..", animate: false);
    }

    [RelayCommand]
    private async Task SelectExercise(ExerciseCatalogItemPresentationModel? exercise)
    {
        if (exercise is null)
            return;

        await Shell.Current.GoToAsync(
            $"..?catalogExerciseId={Uri.EscapeDataString(exercise.Id)}",
            animate: false);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedForce = ExercisePresentationOptions.AllFilterLabel;
        SelectedBodyCategory = ExercisePresentationOptions.AllFilterLabel;
        SelectedMechanic = ExercisePresentationOptions.AllFilterLabel;
        SelectedEquipment = ExercisePresentationOptions.AllFilterLabel;
        SelectedLimbInvolvement = ExercisePresentationOptions.AllFilterLabel;

        IsForceExpanded = false;
        IsBodyCategoryExpanded = false;
        IsMechanicExpanded = false;
        IsEquipmentExpanded = false;
        IsLimbInvolvementExpanded = false;

        ApplyFilters(resetLoadedItems: true);
    }

    [RelayCommand]
    private void LoadMore()
    {
        AppendNextPage();
    }

    [RelayCommand]
    private void SelectForce(string? value)
    {
        SelectedForce = NormalizeFilterValue(value);
        IsForceExpanded = false;
        ApplyFilters(resetLoadedItems: true);
    }

    [RelayCommand]
    private void SelectBodyCategory(string? value)
    {
        SelectedBodyCategory = NormalizeFilterValue(value);
        IsBodyCategoryExpanded = false;
        ApplyFilters(resetLoadedItems: true);
    }

    [RelayCommand]
    private void SelectMechanic(string? value)
    {
        SelectedMechanic = NormalizeFilterValue(value);
        IsMechanicExpanded = false;
        ApplyFilters(resetLoadedItems: true);
    }

    [RelayCommand]
    private void SelectEquipment(string? value)
    {
        SelectedEquipment = NormalizeFilterValue(value);
        IsEquipmentExpanded = false;
        ApplyFilters(resetLoadedItems: true);
    }

    [RelayCommand]
    private void SelectLimbInvolvement(string? value)
    {
        SelectedLimbInvolvement = NormalizeFilterValue(value);
        IsLimbInvolvementExpanded = false;
        ApplyFilters(resetLoadedItems: true);
    }

    partial void OnSearchTextChanged(string value)
    {
        DebounceSearch();
    }

    private async void DebounceSearch()
    {
        searchDebounceCts?.Cancel();
        searchDebounceCts?.Dispose();

        var cts = new CancellationTokenSource();
        searchDebounceCts = cts;

        try
        {
            await Task.Delay(220, cts.Token);

            if (!cts.IsCancellationRequested)
                MainThread.BeginInvokeOnMainThread(() => ApplyFilters(resetLoadedItems: true));
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void ApplyFilters(bool resetLoadedItems)
    {
        var query = SearchText.Trim();

        currentFilteredItems.Clear();

        currentFilteredItems.AddRange(
            allItems
                .Where(item =>
                    MatchesSearch(item, query) &&
                    MatchesExact(item.Metadata.ForceText, SelectedForce) &&
                    MatchesExact(item.Metadata.BodyCategoryText, SelectedBodyCategory) &&
                    MatchesExact(item.Metadata.MechanicText, SelectedMechanic) &&
                    MatchesExact(item.Metadata.EquipmentText, SelectedEquipment) &&
                    MatchesExact(item.Metadata.LimbInvolvementText, SelectedLimbInvolvement))
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase));

        ResultCountText = currentFilteredItems.Count == 1
            ? "1 exercise"
            : $"{currentFilteredItems.Count} exercises";

        if (resetLoadedItems)
        {
            loadedCount = 0;
            FilteredExercises.Clear();
        }

        AppendNextPage();
    }

    private void AppendNextPage()
    {
        if (loadedCount >= currentFilteredItems.Count)
        {
            CanLoadMore = false;
            return;
        }

        var nextItems = currentFilteredItems
            .Skip(loadedCount)
            .Take(PageSize)
            .ToList();

        foreach (var item in nextItems)
            FilteredExercises.Add(item);

        loadedCount += nextItems.Count;
        CanLoadMore = loadedCount < currentFilteredItems.Count;
    }

    private static bool MatchesSearch(ExerciseCatalogItemPresentationModel item, string query)
    {
        return string.IsNullOrWhiteSpace(query)
            || item.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesExact(string value, string selected)
    {
        return string.IsNullOrWhiteSpace(selected)
            || selected == ExercisePresentationOptions.AllFilterLabel
            || string.Equals(value, selected, StringComparison.OrdinalIgnoreCase);
    }

    private void RebuildOptions()
    {
        FillOptions(ForceOptions, allItems.Select(item => item.Metadata.ForceText));
        FillOptions(BodyCategoryOptions, allItems.Select(item => item.Metadata.BodyCategoryText));
        FillOptions(MechanicOptions, allItems.Select(item => item.Metadata.MechanicText));
        FillOptions(EquipmentOptions, allItems.Select(item => item.Metadata.EquipmentText));
        FillOptions(LimbInvolvementOptions, allItems.Select(item => item.Metadata.LimbInvolvementText));
    }

    private static void FillOptions(ObservableCollection<string> target, IEnumerable<string> values)
    {
        target.Clear();
        target.Add(ExercisePresentationOptions.AllFilterLabel);

        foreach (var value in values
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
        {
            target.Add(value);
        }
    }

    private static string NormalizeFilterValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? ExercisePresentationOptions.AllFilterLabel
            : value;
    }

    private static ExerciseCatalogItemPresentationModel ToPresentationModel(ExerciseCatalogItemModel model)
    {
        var metadata = new ExerciseMetadataPresentationModel
        {
            Force = model.Force,
            BodyCategory = model.BodyCategory,
            Mechanic = model.Mechanic,
            Equipment = model.Equipment,
            LimbInvolvement = model.LimbInvolvement,
            MovementPattern = model.MovementPattern
        };

        CopyStrings(model.PrimaryMuscleCategories, metadata.PrimaryMuscleCategories);
        CopyStrings(model.SecondaryMuscleCategories, metadata.SecondaryMuscleCategories);
        CopyStrings(model.PrimaryMuscles, metadata.PrimaryMuscles);
        CopyStrings(model.SecondaryMuscles, metadata.SecondaryMuscles);

        return new ExerciseCatalogItemPresentationModel
        {
            Id = model.Id,
            Name = model.Name,
            Notes = model.Notes,
            ImageSource = model.ImageSource,
            Metadata = metadata,
            Aliases = [.. model.Aliases]
        };
    }

    private static void CopyStrings(IEnumerable<string> source, ObservableCollection<string> target)
    {
        target.Clear();

        foreach (var value in source.Where(value => !string.IsNullOrWhiteSpace(value)))
            target.Add(value);
    }
}