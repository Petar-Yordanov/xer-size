using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.Views.Pages;

namespace XerSize.ViewModels.Routines;

public partial class RoutineManagerPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private bool _isLoaded;

    public ObservableCollection<Routine> Items { get; } = new();

    public RoutineManagerPageViewModel(IRoutineService routineService)
    {
        _routineService = routineService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        await ReloadAsync();
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task AddRoutineAsync()
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var name = await page.DisplayPromptAsync("New Routine", "Routine name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        await _routineService.CreateRoutineAsync(name);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task RenameRoutineAsync(Routine? routine)
    {
        if (routine is null)
            return;

        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var newName = await page.DisplayPromptAsync("Rename Routine", "New name:", initialValue: routine.Name);
        if (string.IsNullOrWhiteSpace(newName))
            return;

        await _routineService.RenameRoutineAsync(routine.Id, newName);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task DeleteRoutineAsync(Routine? routine)
    {
        if (routine is null)
            return;

        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var confirm = await page.DisplayAlertAsync("Delete Routine", $"Delete '{routine.Name}'?", "Delete", "Cancel");
        if (!confirm)
            return;

        await _routineService.DeleteRoutineAsync(routine.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task DuplicateRoutineAsync(Routine? routine)
    {
        if (routine is null)
            return;

        await _routineService.DuplicateRoutineAsync(routine.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task MoveUpAsync(Routine? routine)
    {
        if (routine is null)
            return;

        await _routineService.MoveRoutineUpAsync(routine.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task MoveDownAsync(Routine? routine)
    {
        if (routine is null)
            return;

        await _routineService.MoveRoutineDownAsync(routine.Id);
        await ReloadAsync();
    }

    [RelayCommand]
    private Task OpenWorkoutsAsync(Routine? routine)
    {
        if (routine is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync($"{nameof(WorkoutManagerPage)}?routineId={routine.Id}");
    }

    private async Task ReloadAsync()
    {
        var routines = await _routineService.GetAllAsync();

        Items.Clear();
        foreach (var item in routines)
            Items.Add(item);
    }
}