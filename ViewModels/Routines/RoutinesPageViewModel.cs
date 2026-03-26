using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.Views.Pages;

namespace XerSize.ViewModels.Routines;

public partial class RoutinesPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private bool _isLoaded;

    public ObservableCollection<Routine> Routines { get; } = new();
    public ObservableCollection<Workout> VisibleWorkouts { get; } = new();
    public ObservableCollection<WorkoutExercise> VisibleExercises { get; } = new();

    [ObservableProperty]
    public partial bool IsQuickActionsOpen { get; set; }
    [ObservableProperty] public partial Routine? SelectedRoutine { get; set; }
    [ObservableProperty] public partial Workout? SelectedWorkout { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }

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
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        await ReloadAsync(SelectedRoutine?.Id, SelectedWorkout?.Id);
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task AddWorkoutAsync()
    {
        if (SelectedRoutine is null)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var name = await page.DisplayPromptAsync("New Workout", "Workout name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        await _routineService.AddWorkoutAsync(SelectedRoutine.Id, name);
        await ReloadAsync(SelectedRoutine.Id, null);
    }

    [RelayCommand]
    private async Task ManageRoutineAsync()
    {
        if (SelectedRoutine is null)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var action = await page.DisplayActionSheetAsync(
            SelectedRoutine.Name,
            "Cancel",
            null,
            "Rename Routine",
            "Duplicate Routine",
            "Delete Routine");

        if (action == "Rename Routine")
        {
            var newName = await page.DisplayPromptAsync("Rename Routine", "New name:", initialValue: SelectedRoutine.Name);
            if (!string.IsNullOrWhiteSpace(newName))
                await _routineService.RenameRoutineAsync(SelectedRoutine.Id, newName);
        }
        else if (action == "Duplicate Routine")
        {
            await _routineService.DuplicateRoutineAsync(SelectedRoutine.Id);
        }
        else if (action == "Delete Routine")
        {
            var confirm = await page.DisplayAlertAsync("Delete Routine", $"Delete '{SelectedRoutine.Name}'?", "Delete", "Cancel");
            if (confirm)
                await _routineService.DeleteRoutineAsync(SelectedRoutine.Id);
        }
        else
        {
            return;
        }

        await ReloadAsync();
    }

    [RelayCommand]
    private async Task HandleWorkoutLongPressAsync(Workout? workout)
    {
        if (SelectedRoutine is null || workout is null)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var action = await page.DisplayActionSheetAsync(
            workout.Name,
            "Cancel",
            null,
            "Rename Workout",
            "Duplicate Workout",
            "Delete Workout");

        if (action == "Rename Workout")
        {
            var newName = await page.DisplayPromptAsync("Rename Workout", "New name:", initialValue: workout.Name);
            if (!string.IsNullOrWhiteSpace(newName))
                await _routineService.RenameWorkoutAsync(SelectedRoutine.Id, workout.Id, newName);
        }
        else if (action == "Duplicate Workout")
        {
            await _routineService.DuplicateWorkoutAsync(SelectedRoutine.Id, workout.Id);
        }
        else if (action == "Delete Workout")
        {
            var confirm = await page.DisplayAlertAsync("Delete Workout", $"Delete '{workout.Name}'?", "Delete", "Cancel");
            if (confirm)
                await _routineService.DeleteWorkoutAsync(SelectedRoutine.Id, workout.Id);
        }
        else
        {
            return;
        }

        await ReloadAsync(SelectedRoutine.Id, SelectedWorkout?.Id);
    }

    [RelayCommand]
    private async Task StartWorkoutAsync()
    {
        IsQuickActionsOpen = false;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null || SelectedWorkout is null)
            return;

        await page.DisplayAlert("Start Workout", $"Starting '{SelectedWorkout.Name}' is not implemented yet.", "OK");
    }

    [RelayCommand]
    private async Task OpenWorkoutMoreAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null || SelectedWorkout is null)
            return;

        await page.DisplayAlert("Workout Options", $"More options for '{SelectedWorkout.Name}' are not implemented yet.", "OK");
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

        foreach (var workout in SelectedRoutine.Workouts.OrderBy(x => x.Name))
            VisibleWorkouts.Add(workout);
    }

    private void RefreshExercises()
    {
        VisibleExercises.Clear();

        if (SelectedWorkout is null)
            return;

        foreach (var exercise in SelectedWorkout.Exercises.OrderBy(x => x.SortOrder))
            VisibleExercises.Add(exercise);
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
    private Task EditExerciseAsync(WorkoutExercise exercise)
    {
        if (SelectedRoutine is null || SelectedWorkout is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync(
            $"{nameof(AddExercisePage)}?routineId={SelectedRoutine.Id}&workoutId={SelectedWorkout.Id}&exerciseId={exercise.Id}");
    }

    [RelayCommand]
    private async Task DeleteExerciseAsync(WorkoutExercise exercise)
    {
        if (SelectedRoutine is null || SelectedWorkout is null)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var confirm = await page.DisplayAlertAsync("Delete Exercise", $"Delete '{exercise.Name}'?", "Delete", "Cancel");
        if (!confirm)
            return;

        await _routineService.DeleteExerciseAsync(SelectedRoutine.Id, SelectedWorkout.Id, exercise.Id);
        await ReloadAsync(SelectedRoutine.Id, SelectedWorkout.Id);
    }

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