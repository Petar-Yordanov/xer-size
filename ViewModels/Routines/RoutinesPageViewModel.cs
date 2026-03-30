using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.Views.Pages;

namespace XerSize.ViewModels.Routines;

public partial class RoutinesPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;

    public ObservableCollection<Routine> Routines { get; } = new();
    public ObservableCollection<Workout> VisibleWorkouts { get; } = new();
    public ObservableCollection<WorkoutExerciseCardRow> VisibleExercises { get; } = new();

    [ObservableProperty]
    public partial bool IsQuickActionsOpen { get; set; }

    [ObservableProperty]
    public partial bool IsRoutineSheetOpen { get; set; }

    [ObservableProperty]
    public partial Routine? SelectedRoutine { get; set; }

    [ObservableProperty]
    public partial Workout? SelectedWorkout { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public string SelectedRoutineName => SelectedRoutine?.Name ?? "Select routine";
    public string SelectedWorkoutName => SelectedWorkout?.Name ?? "None";

    public RoutinesPageViewModel(IRoutineService routineService)
    {
        _routineService = routineService;
    }

    [RelayCommand]
    private void ToggleQuickActions()
    {
        IsQuickActionsOpen = !IsQuickActionsOpen;
    }

    [RelayCommand]
    private void CloseQuickActions()
    {
        IsQuickActionsOpen = false;
    }

    [RelayCommand]
    private void ToggleRoutineSheet()
    {
        IsRoutineSheetOpen = !IsRoutineSheetOpen;
    }

    [RelayCommand]
    private void CloseRoutineSheet()
    {
        IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    private void SelectRoutine(Routine? routine)
    {
        if (routine is null)
            return;

        SelectedRoutine = routine;
        IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await ReloadAsync(SelectedRoutine?.Id, SelectedWorkout?.Id);
    }

    [RelayCommand]
    private async Task AddRoutineAsync()
    {
        IsRoutineSheetOpen = false;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var name = await page.DisplayPromptAsync("New Routine", "Routine name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        var created = await _routineService.CreateRoutineAsync(name);
        await ReloadAsync(created.Id, null);
    }

    [RelayCommand]
    private async Task AddWorkoutAsync()
    {
        if (SelectedRoutine is null)
            return;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var name = await page.DisplayPromptAsync("New Workout", "Workout name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        await _routineService.AddWorkoutAsync(SelectedRoutine.Id, name);
        await ReloadAsync(SelectedRoutine.Id, null);
    }

    [RelayCommand]
    private Task HandleRoutineLongPressAsync()
    {
        IsRoutineSheetOpen = false;
        return Shell.Current.GoToAsync(nameof(RoutineManagerPage));
    }

    [RelayCommand]
    private Task HandleWorkoutLongPressAsync(Workout? workout)
    {
        if (SelectedRoutine is null || workout is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync($"{nameof(WorkoutManagerPage)}?routineId={SelectedRoutine.Id}");
    }

    [RelayCommand]
    private async Task StartWorkoutAsync()
    {
        IsQuickActionsOpen = false;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null || SelectedWorkout is null)
            return;

        await page.DisplayAlert("Start Workout", $"Starting '{SelectedWorkout.Name}' is not implemented yet.", "OK");
    }

    [RelayCommand]
    private async Task EditExerciseAsync(WorkoutExerciseCardRow? row)
    {
        if (row is null || SelectedRoutine is null || SelectedWorkout is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(AddExercisePage)}?routineId={SelectedRoutine.Id}&workoutId={SelectedWorkout.Id}&exerciseId={row.Exercise.Id}");
    }

    [RelayCommand]
    private void ToggleExerciseExpanded(WorkoutExerciseCardRow? row)
    {
        if (row is null)
            return;

        row.IsExpanded = !row.IsExpanded;
    }

    [RelayCommand]
    private async Task OpenWorkoutStatisticsAsync()
    {
        IsQuickActionsOpen = false;

        if (SelectedRoutine is null || SelectedWorkout is null)
            return;

        await Shell.Current.GoToAsync(
            $"//statistics?routineId={SelectedRoutine.Id}&workoutId={SelectedWorkout.Id}&tab=preferences");
    }

    private async Task ReloadAsync(Guid? routineId = null, Guid? workoutId = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var allRoutines = await _routineService.GetAllAsync();

            Routines.Clear();
            foreach (var item in allRoutines)
                Routines.Add(item);

            var resolvedRoutine = routineId.HasValue
                ? Routines.FirstOrDefault(x => x.Id == routineId.Value)
                : Routines.FirstOrDefault();

            SelectedRoutine = resolvedRoutine;

            RefreshWorkouts();

            var resolvedWorkout = SelectedRoutine?.Workouts.FirstOrDefault(x => x.Id == workoutId)
                                 ?? SelectedRoutine?.Workouts.FirstOrDefault();

            SelectedWorkout = resolvedWorkout;

            RefreshExercises();
            OnPropertyChanged(nameof(SelectedRoutineName));
            OnPropertyChanged(nameof(SelectedWorkoutName));
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedRoutineChanged(Routine? value)
    {
        RefreshWorkouts();

        if (value is not null && (SelectedWorkout is null || value.Workouts.All(x => x.Id != SelectedWorkout.Id)))
            SelectedWorkout = value.Workouts.FirstOrDefault();

        RefreshExercises();
        OnPropertyChanged(nameof(SelectedRoutineName));
        OnPropertyChanged(nameof(SelectedWorkoutName));
    }

    partial void OnSelectedWorkoutChanged(Workout? value)
    {
        RefreshExercises();
        OnPropertyChanged(nameof(SelectedWorkoutName));
    }

    private void RefreshWorkouts()
    {
        VisibleWorkouts.Clear();

        if (SelectedRoutine is null)
            return;

        foreach (var workout in SelectedRoutine.Workouts)
            VisibleWorkouts.Add(workout);
    }

    private void RefreshExercises()
    {
        VisibleExercises.Clear();

        if (SelectedWorkout is null)
            return;

        foreach (var exercise in SelectedWorkout.Exercises.OrderBy(x => x.SortOrder))
        {
            VisibleExercises.Add(new WorkoutExerciseCardRow(
                exercise,
                onEdit: EditExerciseAsync,
                onDelete: DeleteExerciseAsync));
        }
    }

    [RelayCommand]
    private Task OpenSettingsAsync() => Shell.Current.GoToAsync(nameof(SettingsPage));

    [RelayCommand]
    private async Task AddExerciseAsync()
    {
        IsQuickActionsOpen = false;

        if (SelectedRoutine is null || SelectedWorkout is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(AddExercisePage)}?routineId={SelectedRoutine.Id}&workoutId={SelectedWorkout.Id}");
    }

    [RelayCommand]
    private async Task DeleteExerciseAsync(WorkoutExerciseCardRow? row)
    {
        if (row is null || SelectedRoutine is null || SelectedWorkout is null)
            return;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var confirm = await page.DisplayAlert("Delete Exercise", $"Delete '{row.Name}'?", "Delete", "Cancel");
        if (!confirm)
            return;

        await _routineService.DeleteExerciseAsync(SelectedRoutine.Id, SelectedWorkout.Id, row.Exercise.Id);
        await ReloadAsync(SelectedRoutine.Id, SelectedWorkout.Id);
    }
}

public partial class WorkoutExerciseCardRow : ObservableObject
{
    private readonly Func<WorkoutExerciseCardRow, Task>? _onEdit;
    private readonly Func<WorkoutExerciseCardRow, Task>? _onDelete;

    public WorkoutExercise Exercise { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public WorkoutExerciseCardRow(
        WorkoutExercise exercise,
        Func<WorkoutExerciseCardRow, Task>? onEdit = null,
        Func<WorkoutExerciseCardRow, Task>? onDelete = null)
    {
        Exercise = exercise;
        _onEdit = onEdit;
        _onDelete = onDelete;
        GroupedSets = BuildGroupedSets(exercise);
    }

    public string Name => Exercise.Name;

    public string PrimaryMusclesText
    {
        get
        {
            var muscles = Exercise.PrimaryMuscleCategories
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return muscles.Count == 0 ? "No primary muscle groups" : string.Join(", ", muscles);
        }
    }

    public string RestText =>
        Exercise.DefaultRestSeconds.HasValue ? $"Rest {Exercise.DefaultRestSeconds.Value}s" : "No rest set";

    public bool HasImage => !string.IsNullOrWhiteSpace(Exercise.ImagePath);

    public ImageSource? PreviewImageSource =>
        HasImage ? ImageSource.FromFile(Exercise.ImagePath!) : null;

    public string PlaceholderText =>
        string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpperInvariant();

    public string ExpandButtonText => IsExpanded ? "Hide Info" : "Expand Info";

    public IReadOnlyList<WorkoutExerciseGroupedSetRow> GroupedSets { get; }

    partial void OnIsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ExpandButtonText));
    }

    [RelayCommand]
    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (_onEdit is not null)
            await _onEdit(this);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_onDelete is not null)
            await _onDelete(this);
    }

    private static IReadOnlyList<WorkoutExerciseGroupedSetRow> BuildGroupedSets(WorkoutExercise exercise)
    {
        return exercise.Sets
            .OrderBy(x => x.Order)
            .GroupBy(x => new
            {
                Reps = x.Reps,
                WeightKg = x.WeightKg,
                DurationSeconds = x.DurationSeconds
            })
            .Select(group => new WorkoutExerciseGroupedSetRow
            {
                Count = group.Count(),
                Reps = group.Key.Reps,
                WeightKg = group.Key.WeightKg,
                DurationSeconds = group.Key.DurationSeconds
            })
            .ToList();
    }
}

public sealed class WorkoutExerciseGroupedSetRow
{
    public int Count { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }

    public string SummaryText
    {
        get
        {
            var setWord = Count == 1 ? "set" : "sets";

            if (DurationSeconds.HasValue && DurationSeconds.Value > 0)
            {
                if (Reps.HasValue && Reps.Value > 0)
                    return $"{Count} {setWord} × {Reps.Value} reps × {DurationSeconds.Value}s";

                return $"{Count} {setWord} × {DurationSeconds.Value}s";
            }

            if (Reps.HasValue && WeightKg.HasValue)
                return $"{Count} {setWord} × {Reps.Value} reps × {WeightKg.Value:F1} kg";

            if (Reps.HasValue)
                return $"{Count} {setWord} × {Reps.Value} reps";

            if (WeightKg.HasValue)
                return $"{Count} {setWord} × {WeightKg.Value:F1} kg";

            return $"{Count} {setWord}";
        }
    }
}