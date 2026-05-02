using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Models.Presentation.ExerciseMetadata;
using XerSize.Models.Presentation.History;
using XerSize.Models.Presentation.Navigation;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class HistoryPageViewModel : ObservableObject
{
    private readonly WorkoutHistoryService workoutHistoryService;
    private readonly DashboardStatisticsService dashboardStatisticsService;

    public HistoryPageViewModel(
        WorkoutHistoryService workoutHistoryService,
        DashboardStatisticsService dashboardStatisticsService)
    {
        this.workoutHistoryService = workoutHistoryService;
        this.dashboardStatisticsService = dashboardStatisticsService;

        ApplyDateFilter();
        SyncSelectedNav();
    }

    private DateTime startDate = DateTime.Today.AddMonths(-1);
    private DateTime endDate = DateTime.Today;

    [ObservableProperty]
    public partial string CompletedWorkouts { get; set; } = "0";

    [ObservableProperty]
    public partial string TotalTrainingTime { get; set; } = "0 min";

    [ObservableProperty]
    public partial string TotalVolume { get; set; } = "0 kg";

    [ObservableProperty]
    public partial string TotalCalories { get; set; } = "0 kcal";

    public ObservableCollection<HistoryWorkoutPresentationModel> HistoryItems { get; } = [];

    public ObservableCollection<BottomNavItemPresentationModel> BottomNavItems { get; } =
    [
        new() { Id = "dashboard", Name = "Dashboard", IconSource = "statistics.png" },
        new() { Id = "workouts", Name = "Workouts", IconSource = "exercises.png" },
        new() { Id = "history", Name = "History", IconSource = "history.png", IsSelected = true },
        new() { Id = "settings", Name = "Settings", IconSource = "settings.png" }
    ];

    public BottomNavItemPresentationModel DashboardNavItem => BottomNavItems[0];

    public BottomNavItemPresentationModel WorkoutsNavItem => BottomNavItems[1];

    public BottomNavItemPresentationModel CalendarNavItem => BottomNavItems[2];

    public BottomNavItemPresentationModel SettingsNavItem => BottomNavItems[3];

    public DateTime StartDate
    {
        get => startDate;
        set
        {
            var normalized = value.Date;

            if (!SetProperty(ref startDate, normalized))
                return;

            if (normalized > EndDate.Date)
            {
                EndDate = normalized;
                return;
            }

            ApplyDateFilter();
        }
    }

    public DateTime EndDate
    {
        get => endDate;
        set
        {
            var normalized = value.Date;

            if (!SetProperty(ref endDate, normalized))
                return;

            if (normalized < StartDate.Date)
            {
                StartDate = normalized;
                return;
            }

            ApplyDateFilter();
        }
    }

    public void SyncSelectedNav()
    {
        foreach (var nav in BottomNavItems)
            nav.IsSelected = nav.Id == "history";

        OnPropertyChanged(nameof(DashboardNavItem));
        OnPropertyChanged(nameof(WorkoutsNavItem));
        OnPropertyChanged(nameof(CalendarNavItem));
        OnPropertyChanged(nameof(SettingsNavItem));
    }

    [RelayCommand]
    private void ResetDateFilter()
    {
        startDate = DateTime.Today.AddMonths(-1);
        endDate = DateTime.Today;

        OnPropertyChanged(nameof(StartDate));
        OnPropertyChanged(nameof(EndDate));

        ApplyDateFilter();
    }

    [RelayCommand]
    private void ToggleHistoryWorkout(HistoryWorkoutPresentationModel? item)
    {
        if (item is not null)
            item.IsExpanded = !item.IsExpanded;
    }

    [RelayCommand]
    private void ToggleHistoryExercise(HistoryExercisePresentationModel? item)
    {
        if (item is not null)
            item.IsExpanded = !item.IsExpanded;
    }

    [RelayCommand]
    private async Task SelectNav(BottomNavItemPresentationModel? item)
    {
        if (item is null)
            return;

        if (item.Id == "dashboard")
            await Shell.Current.GoToAsync($"//{AppShell.DashboardRoute}", true);
        else if (item.Id == "workouts")
            await Shell.Current.GoToAsync(AppShell.WorkoutsRoute, true);
        else if (item.Id == "settings")
            await Shell.Current.GoToAsync(AppShell.SettingsRoute, true);
        else if (item.Id == "history")
            SyncSelectedNav();
    }

    private void ApplyDateFilter()
    {
        var from = StartDate.Date;
        var to = EndDate.Date.AddDays(1).AddTicks(-1);

        var history = workoutHistoryService.GetHistory(from, to);

        HistoryItems.Clear();

        var totalCalories = 0d;

        foreach (var workout in history)
        {
            var presentation = ToPresentationModel(workout);
            totalCalories += presentation.EstimatedCaloriesBurned;

            HistoryItems.Add(presentation);
        }

        CompletedWorkouts = history.Count.ToString();
        TotalTrainingTime = FormatMinutes(history.Sum(workout => Math.Max(0, workout.DurationMinutes)));
        TotalVolume = $"{CalculateComputedVolumeKg(history):0.#} kg";
        TotalCalories = $"{totalCalories:0} kcal";
    }

    private double CalculateComputedVolumeKg(IEnumerable<HistoryWorkoutItemModel> history)
    {
        var total = 0d;

        foreach (var workout in history.Where(workout => !workout.ExcludeVolumeFromMetrics))
        {
            foreach (var exercise in workoutHistoryService.GetExercises(workout.Id))
            {
                foreach (var set in workoutHistoryService.GetSets(exercise.Id))
                {
                    if (!set.IsCompleted || set.IsSkipped)
                        continue;

                    total += Math.Max(0, set.Reps) * Math.Max(0, set.WeightKg ?? 0);
                }
            }
        }

        return total;
    }

    private HistoryWorkoutPresentationModel ToPresentationModel(HistoryWorkoutItemModel model)
    {
        var presentation = new HistoryWorkoutPresentationModel
        {
            Id = model.Id,
            WorkoutId = model.WorkoutId,
            WorkoutName = model.WorkoutName,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            DurationMinutes = model.DurationMinutes,
            IsPartial = model.IsPartial,
            PlannedSetCount = model.PlannedSetCount,
            EstimatedCaloriesBurned = model.ExcludeCaloriesFromMetrics
                ? 0
                : dashboardStatisticsService.CalculateEstimatedWorkoutCalories(model),
            Notes = model.Notes
        };

        foreach (var exercise in workoutHistoryService.GetExercises(model.Id))
            presentation.Exercises.Add(ToPresentationModel(exercise));

        presentation.NotifyCalculatedPropertiesChanged();

        return presentation;
    }

    private HistoryExercisePresentationModel ToPresentationModel(HistoryExerciseItemModel model)
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
            Name = model.Name,
            Notes = model.Notes,
            ImageSource = model.ImageSource,
            TrackingMode = model.TrackingMode,
            Metadata = metadata,
            CompletedAt = model.CompletedAt
        };

        foreach (var set in workoutHistoryService.GetSets(model.Id))
        {
            presentation.CompletedSets.Add(new HistorySetPresentationModel
            {
                Id = set.Id,
                HistoryExerciseId = set.HistoryExerciseId,
                TrackingMode = model.TrackingMode,
                SortNumber = set.SortNumber,
                Reps = set.Reps,
                WeightKg = set.WeightKg,
                DurationSeconds = set.DurationSeconds,
                DistanceMeters = set.DistanceMeters,
                RestSeconds = set.RestSeconds,
                IsCompleted = set.IsCompleted,
                IsSkipped = set.IsSkipped,
                CompletedAt = set.CompletedAt
            });
        }

        presentation.NotifyCalculatedPropertiesChanged();

        return presentation;
    }

    private static void CopyStrings(IEnumerable<string> source, ObservableCollection<string> target)
    {
        target.Clear();

        foreach (var value in source.Where(value => !string.IsNullOrWhiteSpace(value)))
            target.Add(value);
    }

    private static string FormatMinutes(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} min";

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return remainingMinutes == 0
            ? $"{hours}h"
            : $"{hours}h {remainingMinutes}m";
    }
}