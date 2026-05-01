using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;
using XerSize.Models.Presentation.Workouts;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class ManageWorkoutsPageViewModel : ObservableObject
{
    private readonly WorkoutService workoutService;
    private readonly WorkoutSelectionService workoutSelectionService;

    private WorkoutPresentationModel? editingWorkout;
    private WorkoutPresentationModel? pendingDeleteWorkout;
    private WorkoutPresentationModel? settingsWorkout;
    private bool isLoadingWorkoutSettings;

    public ManageWorkoutsPageViewModel(
        WorkoutService workoutService,
        WorkoutSelectionService workoutSelectionService)
    {
        this.workoutService = workoutService;
        this.workoutSelectionService = workoutSelectionService;

        RefreshWorkouts();
    }

    [ObservableProperty]
    public partial bool IsAddWorkoutDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsEditWorkoutDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsDeleteDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsWorkoutSettingsDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsSettingsTrainingTypeDropdownExpanded { get; set; }

    [ObservableProperty]
    public partial string NewWorkoutName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditWorkoutName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SettingsTrainingType { get; set; } = ExercisePresentationOptions.ToDisplayName(TrainingType.Strength);

    [ObservableProperty]
    public partial bool SettingsExcludeVolumeFromMetrics { get; set; }

    [ObservableProperty]
    public partial bool SettingsExcludeCaloriesFromMetrics { get; set; }

    [ObservableProperty]
    public partial bool SettingsExcludeMetadataFromMetrics { get; set; }

    public ObservableCollection<WorkoutPresentationModel> Workouts { get; } = [];

    public ObservableCollection<string> TrainingTypeOptions { get; } =
        new(Enum.GetValues<TrainingType>().Select(ExercisePresentationOptions.ToDisplayName));

    public string DeleteDialogMessage => pendingDeleteWorkout is null
        ? "This workout and its exercises will be removed."
        : $"Delete {pendingDeleteWorkout.Name}? Its exercises will also be removed.";

    public string WorkoutSettingsDialogMessage => settingsWorkout is null
        ? "Choose how this workout should be categorized in statistics."
        : $"Choose how {settingsWorkout.Name} should be categorized in statistics.";

    partial void OnSettingsTrainingTypeChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return;

        SettingsTrainingType = ExercisePresentationOptions.ToDisplayName(TrainingType.Strength);
    }

    public void RefreshWorkouts()
    {
        var selectedId = workoutSelectionService.SelectedWorkoutId;

        Workouts.Clear();

        foreach (var workout in workoutService.GetWorkouts())
        {
            Workouts.Add(new WorkoutPresentationModel
            {
                Id = workout.Id,
                Name = workout.Name,
                SortNumber = workout.SortNumber,
                TrainingType = workout.TrainingType,
                ExcludeVolumeFromMetrics = workout.ExcludeVolumeFromMetrics,
                ExcludeCaloriesFromMetrics = workout.ExcludeCaloriesFromMetrics,
                ExcludeMetadataFromMetrics = workout.ExcludeMetadataFromMetrics,
                IsSelected = selectedId.HasValue && selectedId.Value == workout.Id
            });
        }

        OnPropertyChanged(nameof(Workouts));
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    private void AddWorkout()
    {
        CloseDialogs();

        NewWorkoutName = string.Empty;
        IsAddWorkoutDialogVisible = true;
    }

    [RelayCommand]
    private void CancelAddWorkout()
    {
        IsAddWorkoutDialogVisible = false;
        NewWorkoutName = string.Empty;
    }

    [RelayCommand]
    private void CreateWorkout()
    {
        var name = NewWorkoutName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        var workout = workoutService.CreateWorkout(name);
        workoutSelectionService.SelectWorkout(workout.Id);

        NewWorkoutName = string.Empty;
        IsAddWorkoutDialogVisible = false;

        RefreshWorkouts();
    }

    [RelayCommand]
    private void EditWorkout(WorkoutPresentationModel? workout)
    {
        if (workout is null)
            return;

        CloseDialogs();

        editingWorkout = workout;
        EditWorkoutName = workout.Name;
        IsEditWorkoutDialogVisible = true;
    }

    [RelayCommand]
    private void CancelEditWorkout()
    {
        IsEditWorkoutDialogVisible = false;
        EditWorkoutName = string.Empty;
        editingWorkout = null;
    }

    [RelayCommand]
    private void SaveEditWorkout()
    {
        var name = EditWorkoutName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (editingWorkout is not null)
            workoutService.RenameWorkout(editingWorkout.Id, name);

        IsEditWorkoutDialogVisible = false;
        EditWorkoutName = string.Empty;
        editingWorkout = null;

        RefreshWorkouts();
    }

    [RelayCommand]
    private void OpenWorkoutSettings(WorkoutPresentationModel? workout)
    {
        if (workout is null)
            return;

        CloseDialogs();

        settingsWorkout = workout;

        isLoadingWorkoutSettings = true;

        SettingsTrainingType = ExercisePresentationOptions.ToDisplayName(workout.TrainingType);
        SettingsExcludeVolumeFromMetrics = workout.ExcludeVolumeFromMetrics;
        SettingsExcludeCaloriesFromMetrics = workout.ExcludeCaloriesFromMetrics;
        SettingsExcludeMetadataFromMetrics = workout.ExcludeMetadataFromMetrics;
        IsSettingsTrainingTypeDropdownExpanded = false;

        isLoadingWorkoutSettings = false;

        OnPropertyChanged(nameof(WorkoutSettingsDialogMessage));

        IsWorkoutSettingsDialogVisible = true;
    }

    [RelayCommand]
    private void SelectSettingsTrainingType(string? trainingType)
    {
        SettingsTrainingType = string.IsNullOrWhiteSpace(trainingType)
            ? ExercisePresentationOptions.ToDisplayName(TrainingType.Strength)
            : trainingType;

        IsSettingsTrainingTypeDropdownExpanded = false;

        SaveWorkoutSettingsImmediately();
    }

    [RelayCommand]
    private void ToggleSettingsExcludeVolume()
    {
        if (isLoadingWorkoutSettings || settingsWorkout is null)
            return;

        SettingsExcludeVolumeFromMetrics = !SettingsExcludeVolumeFromMetrics;

        SaveWorkoutSettingsImmediately();
    }

    [RelayCommand]
    private void ToggleSettingsExcludeCalories()
    {
        if (isLoadingWorkoutSettings || settingsWorkout is null)
            return;

        SettingsExcludeCaloriesFromMetrics = !SettingsExcludeCaloriesFromMetrics;

        SaveWorkoutSettingsImmediately();
    }

    [RelayCommand]
    private void ToggleSettingsExcludeMetadata()
    {
        if (isLoadingWorkoutSettings || settingsWorkout is null)
            return;

        SettingsExcludeMetadataFromMetrics = !SettingsExcludeMetadataFromMetrics;

        SaveWorkoutSettingsImmediately();
    }

    [RelayCommand]
    private void CancelWorkoutSettings()
    {
        IsWorkoutSettingsDialogVisible = false;
        IsSettingsTrainingTypeDropdownExpanded = false;
        settingsWorkout = null;

        OnPropertyChanged(nameof(WorkoutSettingsDialogMessage));
    }

    [RelayCommand]
    private void SaveWorkoutSettings()
    {
        SaveWorkoutSettingsImmediately();
        CancelWorkoutSettings();
    }

    [RelayCommand]
    private void DeleteWorkout(WorkoutPresentationModel? workout)
    {
        if (workout is null)
            return;

        CloseDialogs();

        pendingDeleteWorkout = workout;
        OnPropertyChanged(nameof(DeleteDialogMessage));

        IsDeleteDialogVisible = true;
    }

    [RelayCommand]
    private void CancelDeleteWorkout()
    {
        IsDeleteDialogVisible = false;
        pendingDeleteWorkout = null;

        OnPropertyChanged(nameof(DeleteDialogMessage));
    }

    [RelayCommand]
    private void ConfirmDeleteWorkout()
    {
        var workout = pendingDeleteWorkout;

        IsDeleteDialogVisible = false;
        pendingDeleteWorkout = null;

        OnPropertyChanged(nameof(DeleteDialogMessage));

        if (workout is null)
            return;

        workoutService.RemoveWorkout(workout.Id);

        if (workoutSelectionService.SelectedWorkoutId == workout.Id)
            workoutSelectionService.ClearSelection();

        RefreshWorkouts();

        var firstWorkout = Workouts.FirstOrDefault();

        if (firstWorkout is not null)
        {
            workoutSelectionService.SelectWorkout(firstWorkout.Id);
            firstWorkout.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ReorderWorkout(WorkoutExerciseReorderRequest? request)
    {
        if (request?.SourceItem is not WorkoutPresentationModel source)
            return;

        if (request.TargetItem is not WorkoutPresentationModel target)
            return;

        var sourceIndex = Workouts.IndexOf(source);
        var targetIndex = Workouts.IndexOf(target);

        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        Workouts.Move(sourceIndex, targetIndex);

        workoutService.ReorderWorkouts(Workouts.Select(workout => workout.Id).ToList());

        RefreshWorkouts();
    }

    private void SaveWorkoutSettingsImmediately()
    {
        if (isLoadingWorkoutSettings || settingsWorkout is null)
            return;

        workoutService.UpdateWorkoutSettings(
            settingsWorkout.Id,
            ParseTrainingType(SettingsTrainingType),
            SettingsExcludeVolumeFromMetrics,
            SettingsExcludeCaloriesFromMetrics,
            SettingsExcludeMetadataFromMetrics);

        var updatedWorkout = workoutService.GetWorkout(settingsWorkout.Id);

        if (updatedWorkout is not null)
        {
            settingsWorkout.TrainingType = updatedWorkout.TrainingType;
            settingsWorkout.ExcludeVolumeFromMetrics = updatedWorkout.ExcludeVolumeFromMetrics;
            settingsWorkout.ExcludeCaloriesFromMetrics = updatedWorkout.ExcludeCaloriesFromMetrics;
            settingsWorkout.ExcludeMetadataFromMetrics = updatedWorkout.ExcludeMetadataFromMetrics;
        }

        RefreshWorkouts();
        OnPropertyChanged(nameof(WorkoutSettingsDialogMessage));
    }

    private void CloseDialogs()
    {
        IsAddWorkoutDialogVisible = false;
        IsEditWorkoutDialogVisible = false;
        IsDeleteDialogVisible = false;
        IsWorkoutSettingsDialogVisible = false;
        IsSettingsTrainingTypeDropdownExpanded = false;

        editingWorkout = null;
        pendingDeleteWorkout = null;
        settingsWorkout = null;

        NewWorkoutName = string.Empty;
        EditWorkoutName = string.Empty;

        OnPropertyChanged(nameof(DeleteDialogMessage));
        OnPropertyChanged(nameof(WorkoutSettingsDialogMessage));
    }

    private static TrainingType ParseTrainingType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TrainingType.Strength;

        foreach (var trainingType in Enum.GetValues<TrainingType>())
        {
            if (Normalize(trainingType.ToString()) == Normalize(value))
                return trainingType;

            if (Normalize(ExercisePresentationOptions.ToDisplayName(trainingType)) == Normalize(value))
                return trainingType;
        }

        return TrainingType.Strength;
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
}