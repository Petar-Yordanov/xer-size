using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.Routines;

[QueryProperty(nameof(RoutineIdText), "routineId")]
public partial class WorkoutManagerPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private Guid _routineId;
    private bool _isLoaded;

    public ObservableCollection<Workout> Items { get; } = new();

    [ObservableProperty]
    public partial string RoutineIdText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RoutineName { get; set; } = string.Empty;

    public WorkoutManagerPageViewModel(IRoutineService routineService)
    {
        _routineService = routineService;
    }

    partial void OnRoutineIdTextChanged(string value)
    {
        _isLoaded = false;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        if (!Guid.TryParse(RoutineIdText, out _routineId))
            return;

        await ReloadAsync();
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task AddWorkoutAsync()
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var name = await page.DisplayPromptAsync("New Workout", "Workout name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        await _routineService.AddWorkoutAsync(_routineId, name);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task RenameWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var newName = await page.DisplayPromptAsync("Rename Workout", "New name:", initialValue: workout.Name);
        if (string.IsNullOrWhiteSpace(newName))
            return;

        await _routineService.RenameWorkoutAsync(_routineId, workout.Id, newName);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task DeleteWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var confirm = await page.DisplayAlert("Delete Workout", $"Delete '{workout.Name}'?", "Delete", "Cancel");
        if (!confirm)
            return;

        await _routineService.DeleteWorkoutAsync(_routineId, workout.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task DuplicateWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        await _routineService.DuplicateWorkoutAsync(_routineId, workout.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task MoveUpAsync(Workout? workout)
    {
        if (workout is null)
            return;

        await _routineService.MoveWorkoutUpAsync(_routineId, workout.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task MoveDownAsync(Workout? workout)
    {
        if (workout is null)
            return;

        await _routineService.MoveWorkoutDownAsync(_routineId, workout.Id);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var routines = await _routineService.GetAllAsync();
        var routine = routines.FirstOrDefault(x => x.Id == _routineId);
        if (routine is null)
            return;

        RoutineName = routine.Name;

        Items.Clear();
        foreach (var workout in routine.Workouts)
            Items.Add(workout);
    }
}