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
    [ObservableProperty] public partial string PrimaryMuscleCategoriesText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SecondaryMuscleCategoriesText { get; set; } = string.Empty;

    [ObservableProperty] public partial string RestSecondsText { get; set; } = "90";
    [ObservableProperty] public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImagePath { get; set; } = string.Empty;

    [ObservableProperty] public partial string SelectedSetMode { get; set; } = "Strength";
    [ObservableProperty] public partial string StrengthSetsText { get; set; } = "10x60, 8x70, 6x80";
    [ObservableProperty] public partial string DurationSetsText { get; set; } = "30s, 30s, 45s";

    public IReadOnlyList<string> SetModes { get; } = new[] { "Strength", "Duration" };

    public string PageTitle => IsEditMode ? "Edit Exercise" : "Add Exercise";
    public string SaveButtonText => IsEditMode ? "Save Changes" : "Save Exercise";
    public string SourceSummary =>
        IsCatalogBased
            ? "Catalog exercise selected"
            : "Custom exercise";

    public AddExercisePageViewModel(IExerciseCatalogService catalogService, IRoutineService routineService)
    {
        _catalogService = catalogService;
        _routineService = routineService;
    }

    partial void OnExerciseIdTextChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnCatalogExerciseIdChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnRoutineIdTextChanged(string value) => _ = LoadFromQueryAsync();
    partial void OnWorkoutIdTextChanged(string value) => _ = LoadFromQueryAsync();

    [RelayCommand]
    private void UseCustom()
    {
        IsCatalogBased = false;
        OnPropertyChanged(nameof(SourceSummary));
    }

    [RelayCommand]
    private Task OpenCatalogAsync()
    {
        return Shell.Current.GoToAsync(
            $"{nameof(CatalogExercisePickerPage)}?routineId={RoutineIdText}&workoutId={WorkoutIdText}");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Guid.TryParse(RoutineIdText, out var routineId) || !Guid.TryParse(WorkoutIdText, out var workoutId))
            return;

        var restSeconds = int.TryParse(RestSecondsText, out var rest) ? rest : 90;

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
            PrimaryMuscleCategories = SplitCsv(PrimaryMuscleCategoriesText),
            SecondaryMuscleCategories = SplitCsv(SecondaryMuscleCategoriesText),
            DefaultRestSeconds = restSeconds,
            Notes = Notes,
            ImagePath = ImagePath,
            Sets = SelectedSetMode == "Duration"
                ? ParseDurationSets(DurationSetsText, restSeconds).ToList()
                : ParseStrengthSets(StrengthSetsText, restSeconds).ToList()
        };

        if (IsEditMode)
            await _routineService.UpdateExerciseAsync(routineId, workoutId, exercise);
        else
            await _routineService.AddExerciseAsync(routineId, workoutId, exercise);

        await Shell.Current.GoToAsync("..");
    }

    private async Task LoadFromQueryAsync()
    {
        if (!Guid.TryParse(RoutineIdText, out var routineId) || !Guid.TryParse(WorkoutIdText, out var workoutId))
            return;

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
            PrimaryMuscleCategoriesText = string.Join(", ", catalog.PrimaryMuscleCategories);
            SecondaryMuscleCategoriesText = string.Join(", ", catalog.SecondaryMuscleCategories);

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(SourceSummary));
        }
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
        PrimaryMuscleCategoriesText = string.Join(", ", exercise.PrimaryMuscleCategories);
        SecondaryMuscleCategoriesText = string.Join(", ", exercise.SecondaryMuscleCategories);
        RestSecondsText = (exercise.DefaultRestSeconds ?? 90).ToString();
        Notes = exercise.Notes ?? string.Empty;
        ImagePath = exercise.ImagePath ?? string.Empty;

        var hasDuration = exercise.Sets.Any(x => x.DurationSeconds.HasValue && x.DurationSeconds.Value > 0);
        SelectedSetMode = hasDuration ? "Duration" : "Strength";

        StrengthSetsText = string.Join(", ",
            exercise.Sets
                .OrderBy(x => x.Order)
                .Where(x => x.Reps.HasValue)
                .Select(x => $"{x.Reps!.Value}x{x.WeightKg ?? 0}"));

        DurationSetsText = string.Join(", ",
            exercise.Sets
                .OrderBy(x => x.Order)
                .Where(x => x.DurationSeconds.HasValue)
                .Select(x => $"{x.DurationSeconds!.Value}s"));
    }

    private static List<string> SplitCsv(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static IEnumerable<WorkoutExerciseSet> ParseStrengthSets(string input, int restSeconds)
    {
        var items = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var order = 1;

        foreach (var item in items)
        {
            var parts = item.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var reps) &&
                double.TryParse(parts[1], out var weight))
            {
                yield return new WorkoutExerciseSet
                {
                    Order = order++,
                    Reps = reps,
                    WeightKg = weight,
                    RestSeconds = restSeconds
                };
            }
        }
    }

    private static IEnumerable<WorkoutExerciseSet> ParseDurationSets(string input, int restSeconds)
    {
        var items = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var order = 1;

        foreach (var item in items)
        {
            var cleaned = item.Trim().TrimEnd('s', 'S');
            if (int.TryParse(cleaned, out var seconds))
            {
                yield return new WorkoutExerciseSet
                {
                    Order = order++,
                    DurationSeconds = seconds,
                    WeightKg = 0,
                    RestSeconds = restSeconds
                };
            }
        }
    }
}