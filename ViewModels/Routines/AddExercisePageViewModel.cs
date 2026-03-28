using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.Views.Pages;

namespace XerSize.ViewModels.Routines;

[QueryProperty(nameof(RoutineIdText), "routineId")]
[QueryProperty(nameof(WorkoutIdText), "workoutId")]
[QueryProperty(nameof(ExerciseIdText), "exerciseId")]
[QueryProperty(nameof(CatalogExerciseId), "catalogExerciseId")]
public partial class AddExercisePageViewModel : ObservableObject
{
    private readonly IExerciseCatalogService _catalogService;
    private readonly IRoutineService _routineService;
    private bool _metadataLoaded;

    [ObservableProperty] public partial string RoutineIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WorkoutIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string ExerciseIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string CatalogExerciseId { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsEditMode { get; set; }
    [ObservableProperty] public partial bool IsCatalogBased { get; set; }

    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string Force { get; set; } = string.Empty;
    [ObservableProperty] public partial string BodyCategory { get; set; } = string.Empty;
    [ObservableProperty] public partial string Mechanic { get; set; } = string.Empty;
    [ObservableProperty] public partial string Equipment { get; set; } = string.Empty;
    [ObservableProperty] public partial string LimbInvolvement { get; set; } = string.Empty;
    [ObservableProperty] public partial string MovementPattern { get; set; } = string.Empty;
    [ObservableProperty] public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImagePath { get; set; } = string.Empty;

    [ObservableProperty] public partial string SelectedSetMode { get; set; } = "Strength";

    [ObservableProperty] public partial bool IsAdvancedExpanded { get; set; }
    [ObservableProperty] public partial bool IsPrimaryMusclesSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsSecondaryMusclesSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsPrimaryCategoriesSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsSecondaryCategoriesSheetOpen { get; set; }

    [ObservableProperty] public partial bool IsForceSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsBodyCategorySheetOpen { get; set; }
    [ObservableProperty] public partial bool IsMechanicSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsEquipmentSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsLimbInvolvementSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsMovementPatternSheetOpen { get; set; }

    public ObservableCollection<SingleSelectOptionRow> ForceOptions { get; } = new();
    public ObservableCollection<SingleSelectOptionRow> BodyCategoryOptions { get; } = new();
    public ObservableCollection<SingleSelectOptionRow> MechanicOptions { get; } = new();
    public ObservableCollection<SingleSelectOptionRow> EquipmentOptions { get; } = new();
    public ObservableCollection<SingleSelectOptionRow> LimbInvolvementOptions { get; } = new();
    public ObservableCollection<SingleSelectOptionRow> MovementPatternOptions { get; } = new();

    public ObservableCollection<SelectableOptionRow> PrimaryMuscleOptions { get; } = new();
    public ObservableCollection<SelectableOptionRow> SecondaryMuscleOptions { get; } = new();
    public ObservableCollection<SelectableOptionRow> PrimaryMuscleCategoryOptions { get; } = new();
    public ObservableCollection<SelectableOptionRow> SecondaryMuscleCategoryOptions { get; } = new();

    public ObservableCollection<EditableExerciseSetRow> StrengthSets { get; } = new();
    public ObservableCollection<EditableExerciseSetRow> DurationSets { get; } = new();

    public bool IsStrengthMode => SelectedSetMode == "Strength";
    public bool IsDurationMode => SelectedSetMode == "Duration";
    public bool HasSelectedImage => !string.IsNullOrWhiteSpace(ImagePath);
    public bool ShowImagePlaceholder => !HasSelectedImage;
    public ImageSource? SelectedImageSource => HasSelectedImage ? ImageSource.FromFile(ImagePath) : null;

    public string PageTitle => IsEditMode ? "Edit Exercise" : "Add Exercise";
    public string SaveButtonText => IsEditMode ? "Save Changes" : "Save Exercise";
    public string SourceSummary => IsCatalogBased ? "Catalog exercise selected" : "Custom exercise";

    public string PrimaryMusclesSummary => BuildMultiSummary(PrimaryMuscleOptions, "Select primary muscles");
    public string SecondaryMusclesSummary => BuildMultiSummary(SecondaryMuscleOptions, "Select secondary muscles");
    public string PrimaryMuscleCategoriesSummary => BuildMultiSummary(PrimaryMuscleCategoryOptions, "Select primary categories");
    public string SecondaryMuscleCategoriesSummary => BuildMultiSummary(SecondaryMuscleCategoryOptions, "Select secondary categories");

    public string ForceSummary => BuildSingleSummary(Force, "Select force");
    public string BodyCategorySummary => BuildSingleSummary(BodyCategory, "Select body category");
    public string MechanicSummary => BuildSingleSummary(Mechanic, "Select mechanic");
    public string EquipmentSummary => BuildSingleSummary(Equipment, "Select equipment");
    public string LimbInvolvementSummary => BuildSingleSummary(LimbInvolvement, "Select limb involvement");
    public string MovementPatternSummary => BuildSingleSummary(MovementPattern, "Select movement pattern");

    public string AdvancedButtonText => IsAdvancedExpanded ? "Collapse" : "Expand";

    public AddExercisePageViewModel(IExerciseCatalogService catalogService, IRoutineService routineService)
    {
        _catalogService = catalogService;
        _routineService = routineService;
    }

    partial void OnExerciseIdTextChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnCatalogExerciseIdChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnRoutineIdTextChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnWorkoutIdTextChanged(string value) => _ = LoadFromQueryAsync();

    partial void OnSelectedSetModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsStrengthMode));
        OnPropertyChanged(nameof(IsDurationMode));
    }

    partial void OnImagePathChanged(string value)
    {
        OnPropertyChanged(nameof(HasSelectedImage));
        OnPropertyChanged(nameof(ShowImagePlaceholder));
        OnPropertyChanged(nameof(SelectedImageSource));
    }

    partial void OnIsAdvancedExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(AdvancedButtonText));
    }

    partial void OnForceChanged(string value)
    {
        UpdateSelectedSingleOptions(ForceOptions, value);
        OnPropertyChanged(nameof(ForceSummary));
    }

    partial void OnBodyCategoryChanged(string value)
    {
        UpdateSelectedSingleOptions(BodyCategoryOptions, value);
        OnPropertyChanged(nameof(BodyCategorySummary));
    }

    partial void OnMechanicChanged(string value)
    {
        UpdateSelectedSingleOptions(MechanicOptions, value);
        OnPropertyChanged(nameof(MechanicSummary));
    }

    partial void OnEquipmentChanged(string value)
    {
        UpdateSelectedSingleOptions(EquipmentOptions, value);
        OnPropertyChanged(nameof(EquipmentSummary));
    }

    partial void OnLimbInvolvementChanged(string value)
    {
        UpdateSelectedSingleOptions(LimbInvolvementOptions, value);
        OnPropertyChanged(nameof(LimbInvolvementSummary));
    }

    partial void OnMovementPatternChanged(string value)
    {
        UpdateSelectedSingleOptions(MovementPatternOptions, value);
        OnPropertyChanged(nameof(MovementPatternSummary));
    }

    [RelayCommand]
    private void SelectStrengthMode()
    {
        SelectedSetMode = "Strength";
    }

    [RelayCommand]
    private void SelectDurationMode()
    {
        SelectedSetMode = "Duration";
    }

    [RelayCommand]
    private Task OpenCatalogAsync()
    {
        return Shell.Current.GoToAsync(
            $"{nameof(CatalogExercisePickerPage)}?routineId={RoutineIdText}&workoutId={WorkoutIdText}");
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select exercise image"
            });

            if (result is null)
                return;

            ImagePath = result.FullPath ?? string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private void ToggleAdvanced()
    {
        IsAdvancedExpanded = !IsAdvancedExpanded;
    }

    [RelayCommand]
    private void TogglePrimaryMusclesSheet()
    {
        CloseAllSheets();
        IsPrimaryMusclesSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleSecondaryMusclesSheet()
    {
        CloseAllSheets();
        IsSecondaryMusclesSheetOpen = true;
    }

    [RelayCommand]
    private void TogglePrimaryCategoriesSheet()
    {
        CloseAllSheets();
        IsPrimaryCategoriesSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleSecondaryCategoriesSheet()
    {
        CloseAllSheets();
        IsSecondaryCategoriesSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleForceSheet()
    {
        CloseAllSheets();
        IsForceSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleBodyCategorySheet()
    {
        CloseAllSheets();
        IsBodyCategorySheetOpen = true;
    }

    [RelayCommand]
    private void ToggleMechanicSheet()
    {
        CloseAllSheets();
        IsMechanicSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleEquipmentSheet()
    {
        CloseAllSheets();
        IsEquipmentSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleLimbInvolvementSheet()
    {
        CloseAllSheets();
        IsLimbInvolvementSheetOpen = true;
    }

    [RelayCommand]
    private void ToggleMovementPatternSheet()
    {
        CloseAllSheets();
        IsMovementPatternSheetOpen = true;
    }

    [RelayCommand] private void ClosePrimaryMusclesSheet() => IsPrimaryMusclesSheetOpen = false;
    [RelayCommand] private void CloseSecondaryMusclesSheet() => IsSecondaryMusclesSheetOpen = false;
    [RelayCommand] private void ClosePrimaryCategoriesSheet() => IsPrimaryCategoriesSheetOpen = false;
    [RelayCommand] private void CloseSecondaryCategoriesSheet() => IsSecondaryCategoriesSheetOpen = false;
    [RelayCommand] private void CloseForceSheet() => IsForceSheetOpen = false;
    [RelayCommand] private void CloseBodyCategorySheet() => IsBodyCategorySheetOpen = false;
    [RelayCommand] private void CloseMechanicSheet() => IsMechanicSheetOpen = false;
    [RelayCommand] private void CloseEquipmentSheet() => IsEquipmentSheetOpen = false;
    [RelayCommand] private void CloseLimbInvolvementSheet() => IsLimbInvolvementSheetOpen = false;
    [RelayCommand] private void CloseMovementPatternSheet() => IsMovementPatternSheetOpen = false;

    [RelayCommand]
    private void TogglePrimaryMuscleOption(SelectableOptionRow? option)
    {
        ToggleOption(option);
        OnPropertyChanged(nameof(PrimaryMusclesSummary));
    }

    [RelayCommand]
    private void ToggleSecondaryMuscleOption(SelectableOptionRow? option)
    {
        ToggleOption(option);
        OnPropertyChanged(nameof(SecondaryMusclesSummary));
    }

    [RelayCommand]
    private void TogglePrimaryCategoryOption(SelectableOptionRow? option)
    {
        ToggleOption(option);
        OnPropertyChanged(nameof(PrimaryMuscleCategoriesSummary));
    }

    [RelayCommand]
    private void ToggleSecondaryCategoryOption(SelectableOptionRow? option)
    {
        ToggleOption(option);
        OnPropertyChanged(nameof(SecondaryMuscleCategoriesSummary));
    }

    [RelayCommand]
    private void SelectForce(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        Force = option.Text;
        IsForceSheetOpen = false;
    }

    [RelayCommand]
    private void SelectBodyCategory(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        BodyCategory = option.Text;
        IsBodyCategorySheetOpen = false;
    }

    [RelayCommand]
    private void SelectMechanic(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        Mechanic = option.Text;
        IsMechanicSheetOpen = false;
    }

    [RelayCommand]
    private void SelectEquipment(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        Equipment = option.Text;
        IsEquipmentSheetOpen = false;
    }

    [RelayCommand]
    private void SelectLimbInvolvement(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        LimbInvolvement = option.Text;
        IsLimbInvolvementSheetOpen = false;
    }

    [RelayCommand]
    private void SelectMovementPattern(SingleSelectOptionRow? option)
    {
        if (option is null || string.IsNullOrWhiteSpace(option.Text))
            return;

        MovementPattern = option.Text;
        IsMovementPatternSheetOpen = false;
    }

    [RelayCommand]
    private void AddStrengthSet()
    {
        StrengthSets.Add(new EditableExerciseSetRow
        {
            Order = StrengthSets.Count + 1,
            RestSecondsText = "90"
        });
        Renumber(StrengthSets);
    }

    [RelayCommand]
    private void AddDurationSet()
    {
        DurationSets.Add(new EditableExerciseSetRow
        {
            Order = DurationSets.Count + 1,
            DurationSecondsText = "30",
            RestSecondsText = "45"
        });
        Renumber(DurationSets);
    }

    [RelayCommand]
    private void RemoveStrengthSet(EditableExerciseSetRow? row)
    {
        if (row is null)
            return;

        StrengthSets.Remove(row);
        Renumber(StrengthSets);
    }

    [RelayCommand]
    private void RemoveDurationSet(EditableExerciseSetRow? row)
    {
        if (row is null)
            return;

        DurationSets.Remove(row);
        Renumber(DurationSets);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Guid.TryParse(RoutineIdText, out var routineId) || !Guid.TryParse(WorkoutIdText, out var workoutId))
            return;

        var exercise = new WorkoutExercise
        {
            Id = Guid.TryParse(ExerciseIdText, out var existingExerciseId) ? existingExerciseId : Guid.NewGuid(),
            CatalogExerciseId = IsCatalogBased && !string.IsNullOrWhiteSpace(CatalogExerciseId)
                ? CatalogExerciseId
                : $"Custom_{Guid.NewGuid():N}",
            Name = Name.Trim(),
            Force = Force.Trim(),
            BodyCategory = BodyCategory.Trim(),
            Mechanic = Mechanic.Trim(),
            Equipment = Equipment.Trim(),
            LimbInvolvement = LimbInvolvement.Trim(),
            MovementPattern = string.IsNullOrWhiteSpace(MovementPattern) ? null : MovementPattern.Trim(),
            PrimaryMuscles = GetSelectedTexts(PrimaryMuscleOptions),
            SecondaryMuscles = GetSelectedTexts(SecondaryMuscleOptions),
            PrimaryMuscleCategories = GetSelectedTexts(PrimaryMuscleCategoryOptions),
            SecondaryMuscleCategories = GetSelectedTexts(SecondaryMuscleCategoryOptions),
            Notes = Notes,
            ImagePath = ImagePath,
            DefaultRestSeconds = GetDefaultRestSeconds(),
            Sets = BuildSets().ToList()
        };

        if (exercise.Sets.Count == 0)
            return;

        if (IsEditMode)
            await _routineService.UpdateExerciseAsync(routineId, workoutId, exercise);
        else
            await _routineService.AddExerciseAsync(routineId, workoutId, exercise);

        await Shell.Current.GoToAsync("//routines");
    }

    private async Task LoadFromQueryAsync()
    {
        if (!Guid.TryParse(RoutineIdText, out var routineId) || !Guid.TryParse(WorkoutIdText, out var workoutId))
            return;

        await EnsureMetadataLoadedAsync();

        if (Guid.TryParse(ExerciseIdText, out var exerciseId))
        {
            var existing = await _routineService.GetExerciseAsync(routineId, workoutId, exerciseId);
            if (existing is null)
                return;

            IsEditMode = true;
            FillFromWorkoutExercise(existing);

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(SourceSummary));
            return;
        }

        if (!string.IsNullOrWhiteSpace(CatalogExerciseId))
        {
            var catalog = await _catalogService.GetByIdAsync(CatalogExerciseId);
            if (catalog is null)
                return;

            IsEditMode = false;
            IsCatalogBased = true;

            Name = catalog.Name;
            Force = catalog.Force;
            BodyCategory = catalog.BodyCategory;
            Mechanic = catalog.Mechanic;
            Equipment = catalog.Equipment;
            LimbInvolvement = catalog.LimbInvolvement;
            MovementPattern = catalog.MovementPattern ?? string.Empty;

            SetSelectedValues(PrimaryMuscleOptions, catalog.PrimaryMuscles);
            SetSelectedValues(SecondaryMuscleOptions, catalog.SecondaryMuscles);
            SetSelectedValues(PrimaryMuscleCategoryOptions, catalog.PrimaryMuscleCategories);
            SetSelectedValues(SecondaryMuscleCategoryOptions, catalog.SecondaryMuscleCategories);

            EnsureDefaultRows();
            RaiseSelectionSummaryProperties();
            RaiseSingleSummaryProperties();

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(SourceSummary));
            return;
        }

        EnsureDefaultRows();
        RaiseSelectionSummaryProperties();
        RaiseSingleSummaryProperties();
    }

    private void FillFromWorkoutExercise(WorkoutExercise exercise)
    {
        IsCatalogBased = !string.IsNullOrWhiteSpace(exercise.CatalogExerciseId)
            && !exercise.CatalogExerciseId.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase);

        Name = exercise.Name;
        Force = exercise.Force;
        BodyCategory = exercise.BodyCategory;
        Mechanic = exercise.Mechanic;
        Equipment = exercise.Equipment;
        LimbInvolvement = exercise.LimbInvolvement;
        MovementPattern = exercise.MovementPattern ?? string.Empty;
        Notes = exercise.Notes ?? string.Empty;
        ImagePath = exercise.ImagePath ?? string.Empty;

        SetSelectedValues(PrimaryMuscleOptions, exercise.PrimaryMuscles);
        SetSelectedValues(SecondaryMuscleOptions, exercise.SecondaryMuscles);
        SetSelectedValues(PrimaryMuscleCategoryOptions, exercise.PrimaryMuscleCategories);
        SetSelectedValues(SecondaryMuscleCategoryOptions, exercise.SecondaryMuscleCategories);

        StrengthSets.Clear();
        DurationSets.Clear();

        var hasDuration = exercise.Sets.Any(x => x.DurationSeconds.HasValue && x.DurationSeconds.Value > 0);
        SelectedSetMode = hasDuration ? "Duration" : "Strength";

        foreach (var set in exercise.Sets.OrderBy(x => x.Order))
        {
            if (set.DurationSeconds.HasValue && set.DurationSeconds.Value > 0)
            {
                DurationSets.Add(new EditableExerciseSetRow
                {
                    Order = set.Order,
                    RepsText = set.Reps?.ToString() ?? string.Empty,
                    DurationSecondsText = set.DurationSeconds?.ToString() ?? string.Empty,
                    RestSecondsText = set.RestSeconds?.ToString() ?? string.Empty
                });
            }
            else
            {
                StrengthSets.Add(new EditableExerciseSetRow
                {
                    Order = set.Order,
                    RepsText = set.Reps?.ToString() ?? string.Empty,
                    WeightKgText = set.WeightKg?.ToString() ?? string.Empty,
                    RestSecondsText = set.RestSeconds?.ToString() ?? string.Empty
                });
            }
        }

        if (StrengthSets.Count == 0)
            AddStrengthSet();

        if (DurationSets.Count == 0)
            AddDurationSet();

        Renumber(StrengthSets);
        Renumber(DurationSets);

        RaiseSelectionSummaryProperties();
        RaiseSingleSummaryProperties();
    }

    private IEnumerable<WorkoutExerciseSet> BuildSets()
    {
        var source = IsDurationMode ? DurationSets : StrengthSets;
        var order = 1;

        foreach (var row in source)
        {
            var reps = TryParseInt(row.RepsText);
            var rest = TryParseInt(row.RestSecondsText);

            if (IsDurationMode)
            {
                var duration = TryParseInt(row.DurationSecondsText);
                if (!duration.HasValue)
                    continue;

                yield return new WorkoutExerciseSet
                {
                    Order = order++,
                    Reps = reps,
                    WeightKg = 0,
                    DurationSeconds = duration,
                    RestSeconds = rest
                };
            }
            else
            {
                var weight = TryParseDouble(row.WeightKgText);
                if (!reps.HasValue && !weight.HasValue)
                    continue;

                yield return new WorkoutExerciseSet
                {
                    Order = order++,
                    Reps = reps,
                    WeightKg = weight,
                    DurationSeconds = null,
                    RestSeconds = rest
                };
            }
        }
    }

    private int? GetDefaultRestSeconds()
    {
        var source = IsDurationMode ? DurationSets : StrengthSets;
        return source
            .Select(x => TryParseInt(x.RestSecondsText))
            .FirstOrDefault(x => x.HasValue);
    }

    private void EnsureDefaultRows()
    {
        if (StrengthSets.Count == 0)
        {
            StrengthSets.Add(new EditableExerciseSetRow { Order = 1, RestSecondsText = "90" });
            StrengthSets.Add(new EditableExerciseSetRow { Order = 2, RestSecondsText = "90" });
            StrengthSets.Add(new EditableExerciseSetRow { Order = 3, RestSecondsText = "90" });
        }

        if (DurationSets.Count == 0)
        {
            DurationSets.Add(new EditableExerciseSetRow { Order = 1, DurationSecondsText = "30", RestSecondsText = "45" });
            DurationSets.Add(new EditableExerciseSetRow { Order = 2, DurationSecondsText = "30", RestSecondsText = "45" });
            DurationSets.Add(new EditableExerciseSetRow { Order = 3, DurationSecondsText = "45", RestSecondsText = "45" });
        }

        Renumber(StrengthSets);
        Renumber(DurationSets);
    }

    private async Task EnsureMetadataLoadedAsync()
    {
        if (_metadataLoaded)
            return;

        var catalog = await _catalogService.GetAllAsync();

        RebuildSingleOptions(ForceOptions, catalog.Select(x => x.Force));
        RebuildSingleOptions(BodyCategoryOptions, catalog.Select(x => x.BodyCategory));
        RebuildSingleOptions(MechanicOptions, catalog.Select(x => x.Mechanic));
        RebuildSingleOptions(EquipmentOptions, catalog.Select(x => x.Equipment));
        RebuildSingleOptions(LimbInvolvementOptions, catalog.Select(x => x.LimbInvolvement));
        RebuildSingleOptions(MovementPatternOptions, catalog.Select(x => x.MovementPattern ?? string.Empty));

        RebuildSelectableOptions(PrimaryMuscleOptions, catalog.SelectMany(x => x.PrimaryMuscles));
        RebuildSelectableOptions(SecondaryMuscleOptions, catalog.SelectMany(x => x.SecondaryMuscles));
        RebuildSelectableOptions(PrimaryMuscleCategoryOptions, catalog.SelectMany(x => x.PrimaryMuscleCategories));
        RebuildSelectableOptions(SecondaryMuscleCategoryOptions, catalog.SelectMany(x => x.SecondaryMuscleCategories));

        UpdateSelectedSingleOptions(ForceOptions, Force);
        UpdateSelectedSingleOptions(BodyCategoryOptions, BodyCategory);
        UpdateSelectedSingleOptions(MechanicOptions, Mechanic);
        UpdateSelectedSingleOptions(EquipmentOptions, Equipment);
        UpdateSelectedSingleOptions(LimbInvolvementOptions, LimbInvolvement);
        UpdateSelectedSingleOptions(MovementPatternOptions, MovementPattern);

        _metadataLoaded = true;
    }

    private static void RebuildSingleOptions(ObservableCollection<SingleSelectOptionRow> target, IEnumerable<string> values)
    {
        target.Clear();

        foreach (var value in values
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            target.Add(new SingleSelectOptionRow { Text = value });
        }
    }

    private static void RebuildSelectableOptions(ObservableCollection<SelectableOptionRow> target, IEnumerable<string> values)
    {
        target.Clear();

        foreach (var value in values
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            target.Add(new SelectableOptionRow { Text = value });
        }
    }

    private static void SetSelectedValues(ObservableCollection<SelectableOptionRow> target, IEnumerable<string> selectedValues)
    {
        var selected = selectedValues
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in target)
            item.IsSelected = selected.Contains(item.Text);
    }

    private static void UpdateSelectedSingleOptions(IEnumerable<SingleSelectOptionRow> target, string? selectedValue)
    {
        foreach (var item in target)
            item.IsSelected = !string.IsNullOrWhiteSpace(selectedValue)
                && string.Equals(item.Text, selectedValue, StringComparison.OrdinalIgnoreCase);
    }

    private static void ToggleOption(SelectableOptionRow? option)
    {
        if (option is null)
            return;

        option.IsSelected = !option.IsSelected;
    }

    private static List<string> GetSelectedTexts(IEnumerable<SelectableOptionRow> items)
    {
        return items
            .Where(x => x.IsSelected)
            .Select(x => x.Text)
            .ToList();
    }

    private static string BuildMultiSummary(IEnumerable<SelectableOptionRow> items, string emptyText)
    {
        var selected = items
            .Where(x => x.IsSelected)
            .Select(x => x.Text)
            .ToList();

        if (selected.Count == 0)
            return emptyText;

        if (selected.Count <= 2)
            return string.Join(", ", selected);

        return $"{selected[0]}, {selected[1]} +{selected.Count - 2} more";
    }

    private static string BuildSingleSummary(string? value, string emptyText)
    {
        return string.IsNullOrWhiteSpace(value) ? emptyText : value;
    }

    private void RaiseSelectionSummaryProperties()
    {
        OnPropertyChanged(nameof(PrimaryMusclesSummary));
        OnPropertyChanged(nameof(SecondaryMusclesSummary));
        OnPropertyChanged(nameof(PrimaryMuscleCategoriesSummary));
        OnPropertyChanged(nameof(SecondaryMuscleCategoriesSummary));
    }

    private void RaiseSingleSummaryProperties()
    {
        OnPropertyChanged(nameof(ForceSummary));
        OnPropertyChanged(nameof(BodyCategorySummary));
        OnPropertyChanged(nameof(MechanicSummary));
        OnPropertyChanged(nameof(EquipmentSummary));
        OnPropertyChanged(nameof(LimbInvolvementSummary));
        OnPropertyChanged(nameof(MovementPatternSummary));
    }

    private void CloseAllSheets()
    {
        IsPrimaryMusclesSheetOpen = false;
        IsSecondaryMusclesSheetOpen = false;
        IsPrimaryCategoriesSheetOpen = false;
        IsSecondaryCategoriesSheetOpen = false;
        IsForceSheetOpen = false;
        IsBodyCategorySheetOpen = false;
        IsMechanicSheetOpen = false;
        IsEquipmentSheetOpen = false;
        IsLimbInvolvementSheetOpen = false;
        IsMovementPatternSheetOpen = false;
    }

    private static void Renumber(IList<EditableExerciseSetRow> rows)
    {
        for (int i = 0; i < rows.Count; i++)
            rows[i].Order = i + 1;
    }

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static double? TryParseDouble(string? value)
    {
        return double.TryParse(value, out var parsed) ? parsed : null;
    }
}

public partial class EditableExerciseSetRow : ObservableObject
{
    [ObservableProperty] public partial int Order { get; set; }

    [ObservableProperty] public partial string RepsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WeightKgText { get; set; } = string.Empty;
    [ObservableProperty] public partial string DurationSecondsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string RestSecondsText { get; set; } = string.Empty;

    public string SetLabel => $"Set {Order}";

    partial void OnOrderChanged(int value)
    {
        OnPropertyChanged(nameof(SetLabel));
    }
}

public partial class SelectableOptionRow : ObservableObject
{
    [ObservableProperty] public partial string Text { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSelected { get; set; }
}

public partial class SingleSelectOptionRow : ObservableObject
{
    [ObservableProperty] public partial string Text { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSelected { get; set; }
}