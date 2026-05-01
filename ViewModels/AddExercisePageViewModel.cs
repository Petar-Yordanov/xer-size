using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using XerSize.Models.DataAccessObjects.Catalog;
using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;
using XerSize.Models.Presentation.Sets;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class AddExercisePageViewModel : ObservableObject
{
    private readonly WorkoutService workoutService;
    private readonly ExerciseCatalogService exerciseCatalogService;
    private readonly WorkoutSelectionService workoutSelectionService;

    private Guid? editingWorkoutExerciseId;
    private string? selectedImagePath;
    private string selectedCatalogExerciseId = string.Empty;

    public AddExercisePageViewModel(
        WorkoutService workoutService,
        ExerciseCatalogService exerciseCatalogService,
        WorkoutSelectionService workoutSelectionService)
    {
        this.workoutService = workoutService;
        this.exerciseCatalogService = exerciseCatalogService;
        this.workoutSelectionService = workoutSelectionService;

        SubscribeToOptionChanges(PrimaryMuscleCategories);
        SubscribeToOptionChanges(SecondaryMuscleCategories);
        SubscribeToOptionChanges(PrimaryMuscles);
        SubscribeToOptionChanges(SecondaryMuscles);
    }

    [ObservableProperty]
    public partial string ExerciseName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ImageSource SelectedImagePreviewSource { get; set; } = "add_photo.png";

    [ObservableProperty]
    public partial bool HasSelectedImage { get; set; }

    [ObservableProperty]
    public partial int RestSeconds { get; set; } = 90;

    [ObservableProperty]
    public partial bool IsPrimaryMuscleCategoriesExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsSecondaryMuscleCategoriesExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsPrimaryMusclesExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsSecondaryMusclesExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsAdvancedExpanded { get; set; }

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
    public partial bool IsMovementPatternExpanded { get; set; }

    [ObservableProperty]
    public partial string SelectedForce { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedBodyCategory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedMechanic { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedEquipment { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedLimbInvolvement { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedMovementPattern { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsValidationErrorVisible { get; set; }

    [ObservableProperty]
    public partial string ValidationErrorTitle { get; set; } = "Missing information";

    [ObservableProperty]
    public partial string ValidationErrorMessage { get; set; } = string.Empty;

    public bool IsEditing => editingWorkoutExerciseId.HasValue;

    public string PageTitle => IsEditing ? "Edit exercise" : "Add exercise";

    public string PageSubtitle => IsEditing
        ? "Update this exercise in the workout"
        : "Create a custom exercise for this workout";

    public string SaveButtonText => IsEditing ? "Save changes" : "Save exercise";

    public ObservableCollection<MultiSelectOptionPresentationModel> PrimaryMuscleCategories { get; } =
        CreateMultiSelectOptions(
        [
            "Chest",
            "Back",
            "Shoulders",
            "Arms",
            "Core",
            "Legs",
            "Glutes",
            "Cardio",
            "Mobility"
        ]);

    public ObservableCollection<MultiSelectOptionPresentationModel> SecondaryMuscleCategories { get; } =
        CreateMultiSelectOptions(
        [
            "Chest",
            "Back",
            "Shoulders",
            "Arms",
            "Core",
            "Legs",
            "Glutes",
            "Cardio",
            "Mobility"
        ]);

    public ObservableCollection<MultiSelectOptionPresentationModel> PrimaryMuscles { get; } =
        CreateMultiSelectOptions(
        [
            "Pecs",
            "Lats",
            "Traps",
            "Rear delts",
            "Front delts",
            "Side delts",
            "Biceps",
            "Triceps",
            "Forearms",
            "Abs",
            "Obliques",
            "Quads",
            "Hamstrings",
            "Glutes",
            "Calves"
        ]);

    public ObservableCollection<MultiSelectOptionPresentationModel> SecondaryMuscles { get; } =
        CreateMultiSelectOptions(
        [
            "Pecs",
            "Lats",
            "Traps",
            "Rear delts",
            "Front delts",
            "Side delts",
            "Biceps",
            "Triceps",
            "Forearms",
            "Abs",
            "Obliques",
            "Quads",
            "Hamstrings",
            "Glutes",
            "Calves"
        ]);

    public ObservableCollection<AddEditExerciseSetInputPresentationModel> Sets { get; } = new()
    {
        new() { SortNumber = 0, Reps = "12", WeightKg = "0", RestSeconds = "90" }
    };

    public ObservableCollection<string> ForceOptions { get; } =
        new(ExercisePresentationOptions.Forces.Select(ExercisePresentationOptions.ToDisplayName));

    public ObservableCollection<string> BodyCategoryOptions { get; } =
        new(ExercisePresentationOptions.BodyCategories.Select(ExercisePresentationOptions.ToDisplayName));

    public ObservableCollection<string> MechanicOptions { get; } =
        new(ExercisePresentationOptions.Mechanics.Select(ExercisePresentationOptions.ToDisplayName));

    public ObservableCollection<string> EquipmentOptions { get; } =
        new(ExercisePresentationOptions.Equipment.Select(ExercisePresentationOptions.ToDisplayName));

    public ObservableCollection<string> LimbInvolvementOptions { get; } =
        new(ExercisePresentationOptions.LimbInvolvements.Select(ExercisePresentationOptions.ToDisplayName));

    public ObservableCollection<string> MovementPatternOptions { get; } =
        new(ExercisePresentationOptions.MovementPatterns.Select(ExercisePresentationOptions.ToDisplayName));

    public string PrimaryMuscleCategoriesSummary => BuildSummary(PrimaryMuscleCategories, "Select primary categories");

    public string SecondaryMuscleCategoriesSummary => BuildSummary(SecondaryMuscleCategories, "Select secondary categories");

    public string PrimaryMusclesSummary => BuildSummary(PrimaryMuscles, "Select primary muscles");

    public string SecondaryMusclesSummary => BuildSummary(SecondaryMuscles, "Select secondary muscles");

    public bool HasPrimaryMuscleCategories => PrimaryMuscleCategories.Any(option => option.IsSelected);

    public bool HasSecondaryMuscleCategories => SecondaryMuscleCategories.Any(option => option.IsSelected);

    public bool HasPrimaryMuscles => PrimaryMuscles.Any(option => option.IsSelected);

    public bool HasSecondaryMuscles => SecondaryMuscles.Any(option => option.IsSelected);

    public void LoadForCreate()
    {
        editingWorkoutExerciseId = null;

        ExerciseName = string.Empty;
        Notes = string.Empty;
        selectedCatalogExerciseId = string.Empty;
        selectedImagePath = null;
        SelectedImagePreviewSource = "add_photo.png";
        HasSelectedImage = false;
        RestSeconds = 90;

        SelectedForce = string.Empty;
        SelectedBodyCategory = string.Empty;
        SelectedMechanic = string.Empty;
        SelectedEquipment = string.Empty;
        SelectedLimbInvolvement = string.Empty;
        SelectedMovementPattern = string.Empty;

        ClearSelections(PrimaryMuscleCategories);
        ClearSelections(SecondaryMuscleCategories);
        ClearSelections(PrimaryMuscles);
        ClearSelections(SecondaryMuscles);

        Sets.Clear();
        Sets.Add(new AddEditExerciseSetInputPresentationModel
        {
            SortNumber = 0,
            Reps = "12",
            WeightKg = "0",
            RestSeconds = RestSeconds.ToString(CultureInfo.InvariantCulture)
        });

        NotifyModeChanged();
        RefreshMuscleCategoryState();
        RefreshMuscleState();
    }

    public void LoadForEdit(Guid workoutExerciseId)
    {
        var exercise = workoutService
            .GetWorkouts()
            .SelectMany(workout => workoutService.GetExercises(workout.Id))
            .FirstOrDefault(item => item.Id == workoutExerciseId);

        if (exercise is null)
        {
            ShowValidationError(
                "Exercise not found",
                "The selected exercise no longer exists.");
            return;
        }

        editingWorkoutExerciseId = exercise.Id;
        workoutSelectionService.SelectWorkout(exercise.WorkoutId);

        selectedCatalogExerciseId = exercise.CatalogExerciseId;

        ExerciseName = exercise.Name;
        Notes = exercise.Notes;
        SelectedImagePreviewSource = string.IsNullOrWhiteSpace(exercise.ImageSource)
            ? "image.png"
            : exercise.ImageSource;
        HasSelectedImage = !string.IsNullOrWhiteSpace(exercise.ImageSource) && exercise.ImageSource != "image.png";
        selectedImagePath = HasSelectedImage ? exercise.ImageSource : null;

        SelectedForce = ToDisplayName(exercise.Force);
        SelectedBodyCategory = ToDisplayName(exercise.BodyCategory);
        SelectedMechanic = ToDisplayName(exercise.Mechanic);
        SelectedEquipment = ToDisplayName(exercise.Equipment);
        SelectedLimbInvolvement = ToDisplayName(exercise.LimbInvolvement);
        SelectedMovementPattern = ToDisplayName(exercise.MovementPattern);

        ApplySelectedLabels(PrimaryMuscleCategories, exercise.PrimaryMuscleCategories);
        ApplySelectedLabels(SecondaryMuscleCategories, exercise.SecondaryMuscleCategories);
        ApplySelectedLabels(PrimaryMuscles, exercise.PrimaryMuscles);
        ApplySelectedLabels(SecondaryMuscles, exercise.SecondaryMuscles);

        Sets.Clear();

        var existingSets = workoutService.GetSets(exercise.Id);

        foreach (var set in existingSets)
        {
            Sets.Add(new AddEditExerciseSetInputPresentationModel
            {
                SortNumber = set.SortNumber,
                Reps = set.Reps.ToString(CultureInfo.InvariantCulture),
                WeightKg = set.WeightKg?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty,
                RestSeconds = set.RestSeconds.ToString(CultureInfo.InvariantCulture)
            });
        }

        if (Sets.Count == 0)
        {
            Sets.Add(new AddEditExerciseSetInputPresentationModel
            {
                SortNumber = 0,
                Reps = "10",
                WeightKg = "0",
                RestSeconds = "90"
            });
        }

        RestSeconds = Sets[0].RestSecondsValue;

        NotifyModeChanged();
        RefreshMuscleCategoryState();
        RefreshMuscleState();
    }

    public void ApplyCatalogExercise(string catalogExerciseId)
    {
        var selected = exerciseCatalogService.GetById(catalogExerciseId);

        if (selected is null)
            return;

        ApplyCatalogExercise(selected);
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    private async Task PickImage()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose exercise image",
                FileTypes = FilePickerFileType.Images
            });

            if (result is null)
                return;

            var extension = Path.GetExtension(result.FileName);

            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            var fileName = $"exercise-{Guid.NewGuid():N}{extension}";
            var destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await using var input = await result.OpenReadAsync();
            await using var output = File.Create(destinationPath);

            await input.CopyToAsync(output);

            selectedImagePath = destinationPath;
            SelectedImagePreviewSource = ImageSource.FromFile(destinationPath);
            HasSelectedImage = true;
        }
        catch
        {
            ShowValidationError(
                "Image upload failed",
                "The image could not be selected. Try another image.");
        }
    }

    [RelayCommand]
    private async Task ChooseFrom()
    {
        await Shell.Current.GoToAsync(AppShell.CatalogExercisePickerRoute, true);
    }

    [RelayCommand]
    private void TogglePrimaryMuscleCategory(MultiSelectOptionPresentationModel? item)
    {
        ToggleSelection(item);
        RefreshMuscleCategoryState();
    }

    [RelayCommand]
    private void ToggleSecondaryMuscleCategory(MultiSelectOptionPresentationModel? item)
    {
        ToggleSelection(item);
        RefreshMuscleCategoryState();
    }

    [RelayCommand]
    private void TogglePrimaryMuscle(MultiSelectOptionPresentationModel? item)
    {
        ToggleSelection(item);
        RefreshMuscleState();
    }

    [RelayCommand]
    private void ToggleSecondaryMuscle(MultiSelectOptionPresentationModel? item)
    {
        ToggleSelection(item);
        RefreshMuscleState();
    }

    [RelayCommand]
    private void SelectForce(string? value)
    {
        SelectedForce = value ?? string.Empty;
        IsForceExpanded = false;
    }

    [RelayCommand]
    private void SelectBodyCategory(string? value)
    {
        SelectedBodyCategory = value ?? string.Empty;
        IsBodyCategoryExpanded = false;
    }

    [RelayCommand]
    private void SelectMechanic(string? value)
    {
        SelectedMechanic = value ?? string.Empty;
        IsMechanicExpanded = false;
    }

    [RelayCommand]
    private void SelectEquipment(string? value)
    {
        SelectedEquipment = value ?? string.Empty;
        IsEquipmentExpanded = false;
    }

    [RelayCommand]
    private void SelectLimbInvolvement(string? value)
    {
        SelectedLimbInvolvement = value ?? string.Empty;
        IsLimbInvolvementExpanded = false;
    }

    [RelayCommand]
    private void SelectMovementPattern(string? value)
    {
        SelectedMovementPattern = value ?? string.Empty;
        IsMovementPatternExpanded = false;
    }

    [RelayCommand]
    private void AddSet()
    {
        Sets.Add(new AddEditExerciseSetInputPresentationModel
        {
            SortNumber = Sets.Count,
            Reps = "10",
            WeightKg = "0",
            RestSeconds = RestSeconds.ToString(CultureInfo.InvariantCulture)
        });
    }

    [RelayCommand]
    private void DeleteSet(AddEditExerciseSetInputPresentationModel? set)
    {
        if (set is null)
            return;

        if (Sets.Count <= 1)
        {
            ShowValidationError(
                "At least one set required",
                "Keep one set on the exercise. You can edit its reps and kg.");
            return;
        }

        Sets.Remove(set);
        RenumberSets();
    }

    [RelayCommand]
    private async Task SaveExercise()
    {
        var selectedWorkoutId = workoutSelectionService.SelectedWorkoutId;

        if (!selectedWorkoutId.HasValue)
        {
            ShowValidationError(
                "No workout selected",
                "Select a workout before adding an exercise.");
            return;
        }

        var errors = ValidateInputs(out var parsedSets);

        if (errors.Count > 0)
        {
            ShowValidationError(
                "Fix exercise details",
                string.Join(Environment.NewLine, errors));
            return;
        }

        var exercise = new WorkoutExerciseItemModel
        {
            WorkoutId = selectedWorkoutId.Value,
            CatalogExerciseId = selectedCatalogExerciseId,
            Name = ExerciseName.Trim(),
            Notes = Notes.Trim(),
            ImageSource = string.IsNullOrWhiteSpace(selectedImagePath)
                ? "image.png"
                : selectedImagePath,
            Force = ParseDisplayName<ExerciseForce>(SelectedForce),
            BodyCategory = ParseDisplayName<ExerciseBodyCategory>(SelectedBodyCategory),
            Mechanic = ParseDisplayName<ExerciseMechanic>(SelectedMechanic),
            Equipment = ParseDisplayName<ExerciseEquipment>(SelectedEquipment),
            LimbInvolvement = ParseDisplayName<LimbInvolvement>(SelectedLimbInvolvement),
            MovementPattern = ParseDisplayName<MovementPattern>(SelectedMovementPattern),
            PrimaryMuscleCategories = ToStringList(PrimaryMuscleCategories),
            SecondaryMuscleCategories = ToStringList(SecondaryMuscleCategories),
            PrimaryMuscles = ToStringList(PrimaryMuscles),
            SecondaryMuscles = ToStringList(SecondaryMuscles)
        };

        try
        {
            if (editingWorkoutExerciseId.HasValue)
            {
                workoutService.UpdateExercise(
                    editingWorkoutExerciseId.Value,
                    exercise,
                    parsedSets);
            }
            else
            {
                workoutService.AddCustomExercise(
                    selectedWorkoutId.Value,
                    exercise,
                    parsedSets);
            }

            await Shell.Current.GoToAsync("..", true);
        }
        catch (ServiceValidationException ex)
        {
            ShowValidationError("Could not save exercise", ex.Message);
        }
    }

    [RelayCommand]
    private void CloseValidationError()
    {
        IsValidationErrorVisible = false;
    }

    private void ApplyCatalogExercise(ExerciseCatalogItemModel selected)
    {
        selectedCatalogExerciseId = selected.Id.Trim();

        ExerciseName = selected.Name.Trim();
        Notes = selected.Notes.Trim();

        if (!string.IsNullOrWhiteSpace(selected.ImageSource))
        {
            selectedImagePath = selected.ImageSource;
            SelectedImagePreviewSource = selected.ImageSource;
            HasSelectedImage = selected.ImageSource != "image.png";
        }

        SelectedForce = ToDisplayName(selected.Force);
        SelectedBodyCategory = ToDisplayName(selected.BodyCategory);
        SelectedMechanic = ToDisplayName(selected.Mechanic);
        SelectedEquipment = ToDisplayName(selected.Equipment);
        SelectedLimbInvolvement = ToDisplayName(selected.LimbInvolvement);
        SelectedMovementPattern = ToDisplayName(selected.MovementPattern);

        ApplySelectedLabels(PrimaryMuscleCategories, selected.PrimaryMuscleCategories);
        ApplySelectedLabels(SecondaryMuscleCategories, selected.SecondaryMuscleCategories);
        ApplySelectedLabels(PrimaryMuscles, selected.PrimaryMuscles);
        ApplySelectedLabels(SecondaryMuscles, selected.SecondaryMuscles);

        IsAdvancedExpanded = true;

        RefreshMuscleCategoryState();
        RefreshMuscleState();
    }

    private List<string> ValidateInputs(out List<WorkoutSetModel> parsedSets)
    {
        var errors = new List<string>();
        parsedSets = [];

        if (string.IsNullOrWhiteSpace(ExerciseName))
            errors.Add("Exercise name is required.");

        if (!PrimaryMuscleCategories.Any(option => option.IsSelected))
            errors.Add("Select at least one primary muscle category.");

        if (Sets.Count == 0)
            errors.Add("Add at least one set.");

        if (RestSeconds < 0)
            errors.Add("Rest time cannot be negative.");

        for (var index = 0; index < Sets.Count; index++)
        {
            var set = Sets[index];

            if (!int.TryParse(set.Reps?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var reps))
            {
                errors.Add($"Set {index + 1}: reps must be a whole number.");
                continue;
            }

            if (reps <= 0)
                errors.Add($"Set {index + 1}: reps must be at least 1.");

            if (!TryParseFlexibleDouble(set.WeightKg, out var weightKg))
            {
                errors.Add($"Set {index + 1}: kg must be a number.");
                continue;
            }

            if (weightKg < 0)
                errors.Add($"Set {index + 1}: kg cannot be negative.");

            parsedSets.Add(new WorkoutSetModel
            {
                SortNumber = index,
                Reps = Math.Max(0, reps),
                WeightKg = weightKg,
                RestSeconds = Math.Max(0, RestSeconds)
            });
        }

        return errors;
    }

    private void ShowValidationError(string title, string message)
    {
        ValidationErrorTitle = title;
        ValidationErrorMessage = message;
        IsValidationErrorVisible = true;
    }

    private void NotifyModeChanged()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void SubscribeToOptionChanges(IEnumerable<MultiSelectOptionPresentationModel> options)
    {
        foreach (var option in options)
        {
            option.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MultiSelectOptionPresentationModel.IsSelected))
                {
                    RefreshMuscleCategoryState();
                    RefreshMuscleState();
                }
            };
        }
    }

    private void RefreshMuscleCategoryState()
    {
        OnPropertyChanged(nameof(PrimaryMuscleCategoriesSummary));
        OnPropertyChanged(nameof(SecondaryMuscleCategoriesSummary));
        OnPropertyChanged(nameof(HasPrimaryMuscleCategories));
        OnPropertyChanged(nameof(HasSecondaryMuscleCategories));
    }

    private void RefreshMuscleState()
    {
        OnPropertyChanged(nameof(PrimaryMusclesSummary));
        OnPropertyChanged(nameof(SecondaryMusclesSummary));
        OnPropertyChanged(nameof(HasPrimaryMuscles));
        OnPropertyChanged(nameof(HasSecondaryMuscles));
    }

    private void RenumberSets()
    {
        for (var index = 0; index < Sets.Count; index++)
            Sets[index].SortNumber = index;
    }

    private static void ToggleSelection(MultiSelectOptionPresentationModel? item)
    {
        if (item is not null)
            item.IsSelected = !item.IsSelected;
    }

    private static void ClearSelections(IEnumerable<MultiSelectOptionPresentationModel> options)
    {
        foreach (var option in options)
            option.IsSelected = false;
    }

    private static void ApplySelectedLabels(
        ObservableCollection<MultiSelectOptionPresentationModel> options,
        IEnumerable<string> selectedLabels)
    {
        var selected = selectedLabels
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var label in selected)
        {
            if (options.Any(option => string.Equals(option.Name, label, StringComparison.OrdinalIgnoreCase)))
                continue;

            options.Add(new MultiSelectOptionPresentationModel { Name = label });
        }

        foreach (var option in options)
            option.IsSelected = selected.Contains(option.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildSummary(IEnumerable<MultiSelectOptionPresentationModel> options, string placeholder)
    {
        var selected = options
            .Where(option => option.IsSelected)
            .Select(option => option.Name)
            .ToList();

        if (selected.Count == 0)
            return placeholder;

        if (selected.Count <= 2)
            return string.Join(", ", selected);

        return $"{selected[0]}, {selected[1]} +{selected.Count - 2}";
    }

    private static List<string> ToStringList(IEnumerable<MultiSelectOptionPresentationModel> options)
    {
        return options
            .Where(option => option.IsSelected)
            .Select(option => option.Name)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool TryParseFlexibleDouble(string? value, out double result)
    {
        var text = value?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0;
            return true;
        }

        var normalized = text.Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
               double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
    }

    private static TEnum? ParseDisplayName<TEnum>(string? displayName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return null;

        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (Normalize(value.ToString()) == Normalize(displayName))
                return value;

            if (Normalize(ToDisplayName(value)) == Normalize(displayName))
                return value;
        }

        return null;
    }

    private static string ToDisplayName(ExerciseForce? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName(ExerciseBodyCategory? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName(ExerciseMechanic? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName(ExerciseEquipment? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName(LimbInvolvement? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName(MovementPattern? value)
    {
        return value.HasValue ? ExercisePresentationOptions.ToDisplayName(value.Value) : string.Empty;
    }

    private static string ToDisplayName<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return value switch
        {
            ExerciseForce force => ExercisePresentationOptions.ToDisplayName(force),
            ExerciseBodyCategory bodyCategory => ExercisePresentationOptions.ToDisplayName(bodyCategory),
            ExerciseMechanic mechanic => ExercisePresentationOptions.ToDisplayName(mechanic),
            ExerciseEquipment equipment => ExercisePresentationOptions.ToDisplayName(equipment),
            LimbInvolvement limbInvolvement => ExercisePresentationOptions.ToDisplayName(limbInvolvement),
            MovementPattern movementPattern => ExercisePresentationOptions.ToDisplayName(movementPattern),
            _ => value.ToString()
        };
    }

    private static string Normalize(string value)
    {
        return new string(
            value
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
    }

    private static ObservableCollection<MultiSelectOptionPresentationModel> CreateMultiSelectOptions(IEnumerable<string> labels)
    {
        return new ObservableCollection<MultiSelectOptionPresentationModel>(
            labels.Select(label => new MultiSelectOptionPresentationModel { Name = label }));
    }
}