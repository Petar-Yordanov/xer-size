using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Models.Presentation.ExerciseMetadata;
using XerSize.Models.Presentation.History;
using XerSize.Models.Presentation.Navigation;
using XerSize.Models.Presentation.Workouts;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class WorkoutsPageViewModel : ObservableObject
{
    private readonly WorkoutService workoutService;
    private readonly WorkoutSelectionService workoutSelectionService;
    private readonly UserSettingsService userSettingsService;
    private readonly WorkoutHistoryService workoutHistoryService;
    private readonly ActiveWorkoutService activeWorkoutService;

    private WorkoutExercisePresentationModel? pendingDeleteExercise;

    public WorkoutsPageViewModel(
        WorkoutService workoutService,
        WorkoutSelectionService workoutSelectionService,
        UserSettingsService userSettingsService,
        WorkoutHistoryService workoutHistoryService,
        ActiveWorkoutService activeWorkoutService)
    {
        this.workoutService = workoutService;
        this.workoutSelectionService = workoutSelectionService;
        this.userSettingsService = userSettingsService;
        this.workoutHistoryService = workoutHistoryService;
        this.activeWorkoutService = activeWorkoutService;

        RefreshWorkouts();
        RefreshSelectedWorkout();
        RefreshActiveWorkoutState();
    }

    [ObservableProperty]
    public partial string SelectedWorkoutTitle { get; set; } = "Workouts";

    [ObservableProperty]
    public partial bool IsDeleteDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsQuickActionsMenuVisible { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBottomNavVisible))]
    public partial bool IsAddWorkoutDialogVisible { get; set; }

    [ObservableProperty]
    public partial string NewWorkoutName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsExerciseHistoryDialogVisible { get; set; }

    [ObservableProperty]
    public partial string ExerciseHistoryDialogTitle { get; set; } = "Exercise history";

    [ObservableProperty]
    public partial string ExerciseHistoryDialogMessage { get; set; } = "Previous logged sets for this exercise.";

    [ObservableProperty]
    public partial bool IsStartWorkoutBlockedDialogVisible { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartWorkoutActionTitle))]
    [NotifyPropertyChangedFor(nameof(StartWorkoutActionSubtitle))]
    public partial bool HasActiveWorkout { get; set; }

    public ObservableCollection<WorkoutPresentationModel> WorkoutTabs { get; } = [];

    public ObservableCollection<WorkoutExercisePresentationModel> WorkoutItems { get; } = [];

    public ObservableCollection<HistoryExercisePresentationModel> SelectedExerciseHistoryItems { get; } = [];

    public ObservableCollection<BottomNavItemPresentationModel> BottomNavItems { get; } =
    [
        new() { Id = "dashboard", Name = "Dashboard", IconSource = "statistics.png" },
        new() { Id = "workouts", Name = "Workouts", IconSource = "exercises.png", IsSelected = true },
        new() { Id = "history", Name = "History", IconSource = "history.png" },
        new() { Id = "settings", Name = "Settings", IconSource = "settings.png" }
    ];

    public BottomNavItemPresentationModel DashboardNavItem => BottomNavItems[0];

    public BottomNavItemPresentationModel WorkoutsNavItem => BottomNavItems[1];

    public BottomNavItemPresentationModel CalendarNavItem => BottomNavItems[2];

    public BottomNavItemPresentationModel SettingsNavItem => BottomNavItems[3];

    public bool IsBottomNavVisible => !IsAddWorkoutDialogVisible;

    public bool HasSelectedExerciseHistory => SelectedExerciseHistoryItems.Count > 0;

    public bool HasNoSelectedExerciseHistory => !HasSelectedExerciseHistory;

    public string StartWorkoutActionTitle => HasActiveWorkout
        ? "Continue workout"
        : "Start workout";

    public string StartWorkoutActionSubtitle => HasActiveWorkout
        ? "Return to your active session"
        : "Begin logging this session";

    public string DeleteDialogMessage =>
        pendingDeleteExercise is null
            ? "This exercise will be removed from the workout."
            : $"Remove {pendingDeleteExercise.Name} from {SelectedWorkoutTitle}?";

    public void SyncSelectedNav()
    {
        foreach (var nav in BottomNavItems)
            nav.IsSelected = nav.Id == "workouts";

        OnPropertyChanged(nameof(DashboardNavItem));
        OnPropertyChanged(nameof(WorkoutsNavItem));
        OnPropertyChanged(nameof(CalendarNavItem));
        OnPropertyChanged(nameof(SettingsNavItem));
    }

    public void RefreshWorkouts()
    {
        var workouts = workoutService.GetWorkouts();

        if (!workouts.Any())
        {
            var defaultWorkout = workoutService.CreateWorkout("Workout");
            workoutSelectionService.SelectWorkout(defaultWorkout.Id);
            workouts = workoutService.GetWorkouts();
        }

        var selectedWorkoutId = workoutSelectionService.SelectedWorkoutId;

        if (!selectedWorkoutId.HasValue || workouts.All(workout => workout.Id != selectedWorkoutId.Value))
        {
            selectedWorkoutId = workouts.FirstOrDefault()?.Id;

            if (selectedWorkoutId.HasValue)
                workoutSelectionService.SelectWorkout(selectedWorkoutId.Value);
        }

        WorkoutTabs.Clear();

        foreach (var workout in workouts)
        {
            WorkoutTabs.Add(new WorkoutPresentationModel
            {
                Id = workout.Id,
                Name = workout.Name,
                SortNumber = workout.SortNumber,
                TrainingType = workout.TrainingType,
                ExcludeVolumeFromMetrics = workout.ExcludeVolumeFromMetrics,
                ExcludeCaloriesFromMetrics = workout.ExcludeCaloriesFromMetrics,
                ExcludeMetadataFromMetrics = workout.ExcludeMetadataFromMetrics,
                IsSelected = selectedWorkoutId.HasValue && selectedWorkoutId.Value == workout.Id
            });
        }

        OnPropertyChanged(nameof(WorkoutTabs));
        RefreshActiveWorkoutState();
    }

    public void RefreshSelectedWorkout()
    {
        RefreshWorkouts();

        var selectedWorkoutId = workoutSelectionService.SelectedWorkoutId;
        var selected = selectedWorkoutId.HasValue
            ? workoutService.GetWorkout(selectedWorkoutId.Value)
            : null;

        if (selected is null)
        {
            SelectedWorkoutTitle = "Workouts";
            WorkoutItems.Clear();
            OnPropertyChanged(nameof(WorkoutItems));
            RefreshActiveWorkoutState();
            return;
        }

        SelectedWorkoutTitle = selected.Name;

        var exercises = workoutService
            .GetExercises(selected.Id)
            .Select(ToPresentationModel)
            .ToList();

        ApplyExerciseCardSettings(exercises);

        WorkoutItems.Clear();

        foreach (var exercise in exercises)
            WorkoutItems.Add(exercise);

        OnPropertyChanged(nameof(WorkoutItems));
        RefreshActiveWorkoutState();
    }

    [RelayCommand]
    private async Task ManageWorkouts()
    {
        CloseOverlays();

        await Shell.Current.GoToAsync(AppShell.ManageWorkoutsRoute, true);
    }

    [RelayCommand]
    private void SelectWorkoutTab(WorkoutPresentationModel? tab)
    {
        if (tab is null)
            return;

        CloseOverlays();

        workoutSelectionService.SelectWorkout(tab.Id);

        foreach (var workoutTab in WorkoutTabs)
            workoutTab.IsSelected = workoutTab.Id == tab.Id;

        RefreshSelectedWorkout();
    }

    [RelayCommand]
    private void ReorderWorkoutTab(WorkoutExerciseReorderRequest? request)
    {
        if (request?.SourceItem is not WorkoutPresentationModel source)
            return;

        if (request.TargetItem is not WorkoutPresentationModel target)
            return;

        var sourceIndex = WorkoutTabs.IndexOf(source);
        var targetIndex = WorkoutTabs.IndexOf(target);

        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        WorkoutTabs.Move(sourceIndex, targetIndex);
        workoutService.ReorderWorkouts(WorkoutTabs.Select(workout => workout.Id).ToList());

        RefreshWorkouts();
    }

    [RelayCommand]
    private async Task SelectNav(BottomNavItemPresentationModel? item)
    {
        if (item is null)
            return;

        CloseOverlays();

        if (item.Id == "dashboard")
            await Shell.Current.GoToAsync($"//{AppShell.DashboardRoute}", true);
        else if (item.Id == "history")
            await Shell.Current.GoToAsync(AppShell.HistoryRoute, true);
        else if (item.Id == "settings")
            await Shell.Current.GoToAsync(AppShell.SettingsRoute, true);
        else if (item.Id == "workouts")
            SyncSelectedNav();
    }

    [RelayCommand]
    private void ToggleExpand(WorkoutExercisePresentationModel? item)
    {
        if (item is not null)
            item.IsExpanded = !item.IsExpanded;
    }

    [RelayCommand]
    private void ReorderExercise(WorkoutExerciseReorderRequest? request)
    {
        if (request?.SourceItem is not WorkoutExercisePresentationModel source)
            return;

        if (request.TargetItem is not WorkoutExercisePresentationModel target)
            return;

        var selectedWorkoutId = workoutSelectionService.SelectedWorkoutId;

        if (!selectedWorkoutId.HasValue)
            return;

        var sourceIndex = WorkoutItems.IndexOf(source);
        var targetIndex = WorkoutItems.IndexOf(target);

        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        WorkoutItems.Move(sourceIndex, targetIndex);
        workoutService.ReorderExercises(selectedWorkoutId.Value, WorkoutItems.Select(exercise => exercise.Id).ToList());

        RefreshSelectedWorkout();
    }

    [RelayCommand]
    private async Task EditExercise(WorkoutExercisePresentationModel? item)
    {
        if (item is null)
            return;

        CloseOverlays();

        await Shell.Current.GoToAsync($"{AppShell.AddExerciseRoute}?exerciseId={item.Id}", true);
    }

    [RelayCommand]
    private void DeleteExercise(WorkoutExercisePresentationModel? item)
    {
        if (item is null)
            return;

        IsQuickActionsMenuVisible = false;
        IsAddWorkoutDialogVisible = false;
        IsExerciseHistoryDialogVisible = false;
        IsStartWorkoutBlockedDialogVisible = false;

        pendingDeleteExercise = item;
        OnPropertyChanged(nameof(DeleteDialogMessage));

        IsDeleteDialogVisible = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteDialogVisible = false;
        pendingDeleteExercise = null;
        OnPropertyChanged(nameof(DeleteDialogMessage));
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        var item = pendingDeleteExercise;

        IsDeleteDialogVisible = false;
        pendingDeleteExercise = null;
        OnPropertyChanged(nameof(DeleteDialogMessage));

        if (item is null)
            return;

        workoutService.RemoveExercise(item.Id);
        RefreshSelectedWorkout();
    }

    [RelayCommand]
    private void OpenQuickActions()
    {
        RefreshActiveWorkoutState();

        IsAddWorkoutDialogVisible = false;
        IsDeleteDialogVisible = false;
        IsExerciseHistoryDialogVisible = false;
        IsStartWorkoutBlockedDialogVisible = false;
        IsQuickActionsMenuVisible = !IsQuickActionsMenuVisible;
    }

    [RelayCommand]
    private void CloseQuickActions()
    {
        IsQuickActionsMenuVisible = false;
    }

    [RelayCommand]
    private void AddWorkout()
    {
        IsQuickActionsMenuVisible = false;
        IsDeleteDialogVisible = false;
        IsExerciseHistoryDialogVisible = false;
        IsStartWorkoutBlockedDialogVisible = false;
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

        RefreshSelectedWorkout();
    }

    [RelayCommand]
    private async Task AddExercise()
    {
        CloseOverlays();

        await Shell.Current.GoToAsync(AppShell.AddExerciseRoute, true);
    }

    [RelayCommand]
    private async Task StartWorkout()
    {
        CloseOverlays();

        if (activeWorkoutService.GetActiveWorkout() is not null)
        {
            RefreshActiveWorkoutState();
            await Shell.Current.GoToAsync(AppShell.StartWorkoutRoute, true);
            return;
        }

        var selectedWorkoutId = workoutSelectionService.SelectedWorkoutId;
        var exerciseCount = selectedWorkoutId.HasValue
            ? workoutService.GetExercises(selectedWorkoutId.Value).Count
            : 0;

        if (exerciseCount == 0)
        {
            IsStartWorkoutBlockedDialogVisible = true;
            return;
        }

        await Shell.Current.GoToAsync(AppShell.StartWorkoutRoute, true);
    }

    [RelayCommand]
    private void CloseStartWorkoutBlockedDialog()
    {
        IsStartWorkoutBlockedDialogVisible = false;
    }

    [RelayCommand]
    private void ShowExerciseProgress(WorkoutExercisePresentationModel? item)
    {
        if (item is null)
            return;

        CloseOverlays();

        ExerciseHistoryDialogTitle = item.Name;
        ExerciseHistoryDialogMessage = "Previous logged sets for this exercise.";

        LoadExerciseHistory(item.Id, item.CatalogExerciseId, item.Name);

        IsExerciseHistoryDialogVisible = true;
    }

    [RelayCommand]
    private void CloseExerciseHistory()
    {
        IsExerciseHistoryDialogVisible = false;
    }

    private void LoadExerciseHistory(Guid workoutExerciseId, string catalogExerciseId, string exerciseName)
    {
        SelectedExerciseHistoryItems.Clear();

        var historyWorkouts = workoutHistoryService.GetHistory();

        foreach (var historyWorkout in historyWorkouts)
        {
            var matchingExercises = workoutHistoryService
                .GetExercises(historyWorkout.Id)
                .Where(exercise => IsMatchingExercise(exercise, workoutExerciseId, catalogExerciseId, exerciseName))
                .OrderByDescending(exercise => exercise.CompletedAt)
                .ToList();

            foreach (var historyExercise in matchingExercises)
                SelectedExerciseHistoryItems.Add(ToHistoryPresentationModel(historyExercise, historyWorkout));
        }

        OnPropertyChanged(nameof(HasSelectedExerciseHistory));
        OnPropertyChanged(nameof(HasNoSelectedExerciseHistory));
    }

    private HistoryExercisePresentationModel ToHistoryPresentationModel(
        HistoryExerciseItemModel model,
        HistoryWorkoutItemModel historyWorkout)
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

        var presentation = new HistoryExercisePresentationModel
        {
            Id = model.Id,
            HistoryWorkoutId = model.HistoryWorkoutId,
            CatalogExerciseId = model.CatalogExerciseId,
            WorkoutExerciseId = model.WorkoutExerciseId,
            Name = $"{historyWorkout.CompletedAt:dd MMM yyyy} • {historyWorkout.WorkoutName}",
            Notes = model.Notes,
            ImageSource = model.ImageSource,
            Metadata = metadata,
            CompletedAt = model.CompletedAt,
            IsExpanded = true
        };

        foreach (var set in workoutHistoryService.GetSets(model.Id))
        {
            presentation.CompletedSets.Add(new HistorySetPresentationModel
            {
                Id = set.Id,
                HistoryExerciseId = set.HistoryExerciseId,
                SortNumber = set.SortNumber,
                Reps = set.Reps,
                WeightKg = set.WeightKg,
                RestSeconds = set.RestSeconds,
                IsCompleted = set.IsCompleted,
                IsSkipped = set.IsSkipped,
                CompletedAt = set.CompletedAt
            });
        }

        presentation.NotifyCalculatedPropertiesChanged();

        return presentation;
    }

    private static bool IsMatchingExercise(
        HistoryExerciseItemModel historyExercise,
        Guid workoutExerciseId,
        string catalogExerciseId,
        string exerciseName)
    {
        if (historyExercise.WorkoutExerciseId.HasValue && historyExercise.WorkoutExerciseId.Value == workoutExerciseId)
            return true;

        if (!string.IsNullOrWhiteSpace(catalogExerciseId) &&
            string.Equals(historyExercise.CatalogExerciseId, catalogExerciseId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(exerciseName) &&
               string.Equals(historyExercise.Name, exerciseName, StringComparison.OrdinalIgnoreCase);
    }

    private WorkoutExercisePresentationModel ToPresentationModel(Models.DataAccessObjects.Workouts.WorkoutExerciseItemModel model)
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

        var presentation = new WorkoutExercisePresentationModel
        {
            Id = model.Id,
            WorkoutId = model.WorkoutId,
            CatalogExerciseId = model.CatalogExerciseId,
            Name = model.Name,
            Notes = model.Notes,
            SortNumber = model.SortNumber,
            ImageSource = string.IsNullOrWhiteSpace(model.ImageSource) ? "image.png" : model.ImageSource,
            Metadata = metadata
        };

        foreach (var set in workoutService.GetSets(model.Id))
        {
            presentation.Sets.Add(new WorkoutSetPresentationModel
            {
                Id = set.Id,
                WorkoutExerciseId = set.WorkoutExerciseId,
                SortNumber = set.SortNumber,
                Reps = set.Reps,
                WeightKg = set.WeightKg,
                RestSeconds = set.RestSeconds
            });
        }

        presentation.NotifyDisplayPropertiesChanged();

        return presentation;
    }

    private void ApplyExerciseCardSettings(IEnumerable<WorkoutExercisePresentationModel> exercises)
    {
        if (!userSettingsService.AutoExpandExerciseCards())
            return;

        foreach (var exercise in exercises)
            exercise.IsExpanded = true;
    }

    private void RefreshActiveWorkoutState()
    {
        HasActiveWorkout = activeWorkoutService.GetActiveWorkout() is not null;
    }

    private static void CopyStrings(IEnumerable<string> source, ObservableCollection<string> target)
    {
        target.Clear();

        foreach (var value in source.Where(value => !string.IsNullOrWhiteSpace(value)))
            target.Add(value);
    }

    private void CloseOverlays()
    {
        IsQuickActionsMenuVisible = false;
        IsAddWorkoutDialogVisible = false;
        IsDeleteDialogVisible = false;
        IsExerciseHistoryDialogVisible = false;
        IsStartWorkoutBlockedDialogVisible = false;
    }
}