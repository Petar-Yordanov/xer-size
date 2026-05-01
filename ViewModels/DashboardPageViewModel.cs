using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using XerSize.Models.Presentation.Navigation;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class DashboardPageViewModel : ObservableObject
{
    private const string NoChartDataMessage = "There is no data available to show. Start and complete a workout to show graphs.";

    private readonly DashboardStatisticsService dashboardStatisticsService;
    private readonly TrainingAdviceService trainingAdviceService;

    private double sessionTargetActual;
    private double sessionTargetGoal;
    private double calorieTargetActual;
    private double calorieTargetGoal;

    public DashboardPageViewModel(
        DashboardStatisticsService dashboardStatisticsService,
        TrainingAdviceService trainingAdviceService)
    {
        this.dashboardStatisticsService = dashboardStatisticsService;
        this.trainingAdviceService = trainingAdviceService;

        RangeTabs =
        [
            new() { Id = "week", Name = "Week", IsSelected = true },
            new() { Id = "month", Name = "Month" },
            new() { Id = "3m", Name = "3 Months" },
            new() { Id = "6m", Name = "6 Months" },
            new() { Id = "year", Name = "Year" }
        ];

        DashboardNavItem = new BottomNavItemPresentationModel
        {
            Id = "dashboard",
            Name = "Dashboard",
            IconSource = "statistics.png",
            IsSelected = true
        };

        WorkoutsNavItem = new BottomNavItemPresentationModel
        {
            Id = "workouts",
            Name = "Workouts",
            IconSource = "exercises.png"
        };

        CalendarNavItem = new BottomNavItemPresentationModel
        {
            Id = "history",
            Name = "History",
            IconSource = "history.png"
        };

        SettingsNavItem = new BottomNavItemPresentationModel
        {
            Id = "settings",
            Name = "Settings",
            IconSource = "settings.png"
        };

        LoadAdvice();
        LoadDashboard();
        SyncSelectedNav();
    }

    [ObservableProperty]
    public partial string AdviceTitle { get; set; } = "Small habits matter.";

    [ObservableProperty]
    public partial string AdviceSubtitle { get; set; } = "Consistency beats intensity when recovery is managed well.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AverageCaloriesBurned))]
    public partial string AverageEstimatedCaloriesBurned { get; set; } = "0 kcal";

    [ObservableProperty]
    public partial string AverageWorkoutTime { get; set; } = "0m";

    [ObservableProperty]
    public partial string AverageSessionsPerWeek { get; set; } = "0";

    [ObservableProperty]
    public partial string SessionTargetValue { get; set; } = "0 / 0";

    [ObservableProperty]
    public partial string SessionTargetSubtitle { get; set; } = "Selected range";

    [ObservableProperty]
    public partial string CalorieTargetValue { get; set; } = "0 / 0 kcal";

    [ObservableProperty]
    public partial string CalorieTargetSubtitle { get; set; } = "Selected range";

    [ObservableProperty]
    public partial ISeries[] ActivityTimelineSeries { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] ActivityTimelineXAxes { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] ActivityTimelineYAxes { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoActivityTimelineData))]
    public partial bool HasActivityTimelineData { get; set; }

    [ObservableProperty]
    public partial ISeries[] PreferredMuscleGroupsSeries { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoPreferredMuscleGroupsData))]
    public partial bool HasPreferredMuscleGroupsData { get; set; }

    [ObservableProperty]
    public partial ISeries[] PreferredEquipmentSeries { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] PreferredEquipmentXAxes { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] PreferredEquipmentYAxes { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoPreferredEquipmentData))]
    public partial bool HasPreferredEquipmentData { get; set; }

    [ObservableProperty]
    public partial ISeries[] PreferredMechanicSeries { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] PreferredMechanicXAxes { get; set; } = [];

    [ObservableProperty]
    public partial Axis[] PreferredMechanicYAxes { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoPreferredMechanicData))]
    public partial bool HasPreferredMechanicData { get; set; }

    public ObservableCollection<DashboardRangeTabPresentationModel> RangeTabs { get; }

    public BottomNavItemPresentationModel DashboardNavItem { get; }

    public BottomNavItemPresentationModel WorkoutsNavItem { get; }

    public BottomNavItemPresentationModel CalendarNavItem { get; }

    public BottomNavItemPresentationModel SettingsNavItem { get; }

    public string AverageCaloriesBurned => AverageEstimatedCaloriesBurned;

    public string ChartEmptyMessage => NoChartDataMessage;

    public bool HasNoActivityTimelineData => !HasActivityTimelineData;

    public bool HasNoPreferredMuscleGroupsData => !HasPreferredMuscleGroupsData;

    public bool HasNoPreferredEquipmentData => !HasPreferredEquipmentData;

    public bool HasNoPreferredMechanicData => !HasPreferredMechanicData;

    public double SessionTargetProgress => CalculateProgress(sessionTargetActual, sessionTargetGoal);

    public string SessionTargetProgressText => sessionTargetGoal <= 0
        ? "No target"
        : $"{sessionTargetActual:0}/{sessionTargetGoal:0} sessions";

    public double CalorieTargetProgress => CalculateProgress(calorieTargetActual, calorieTargetGoal);

    public string CalorieTargetProgressText => calorieTargetGoal <= 0
        ? "No target"
        : $"{calorieTargetActual:0}/{calorieTargetGoal:0} kcal";

    public void SyncSelectedNav()
    {
        DashboardNavItem.IsSelected = true;
        WorkoutsNavItem.IsSelected = false;
        CalendarNavItem.IsSelected = false;
        SettingsNavItem.IsSelected = false;

        OnPropertyChanged(nameof(DashboardNavItem));
        OnPropertyChanged(nameof(WorkoutsNavItem));
        OnPropertyChanged(nameof(CalendarNavItem));
        OnPropertyChanged(nameof(SettingsNavItem));

        LoadAdvice();
        LoadDashboard();
    }

    [RelayCommand]
    private void SelectRangeTab(DashboardRangeTabPresentationModel? selectedTab)
    {
        if (selectedTab is null)
            return;

        foreach (var tab in RangeTabs)
            tab.IsSelected = ReferenceEquals(tab, selectedTab);

        LoadDashboard();
    }

    [RelayCommand]
    private async Task SelectNav(BottomNavItemPresentationModel? item)
    {
        if (item is null)
            return;

        if (item.Id == "dashboard")
            SyncSelectedNav();
        else if (item.Id == "workouts")
            await Shell.Current.GoToAsync(AppShell.WorkoutsRoute, true);
        else if (item.Id == "history")
            await Shell.Current.GoToAsync(AppShell.HistoryRoute, true);
        else if (item.Id == "settings")
            await Shell.Current.GoToAsync(AppShell.SettingsRoute, true);
    }

    [RelayCommand]
    private static async Task GoToSettings()
    {
        await Shell.Current.GoToAsync(AppShell.SettingsRoute, true);
    }

    [RelayCommand]
    private static async Task GoToWorkouts()
    {
        await Shell.Current.GoToAsync(AppShell.WorkoutsRoute, true);
    }

    private void LoadAdvice()
    {
        var advice = trainingAdviceService.GetRandomAdvice();

        AdviceTitle = advice.Title;
        AdviceSubtitle = advice.Subtitle;
    }

    private void LoadDashboard()
    {
        var selectedRange = RangeTabs.FirstOrDefault(tab => tab.IsSelected)?.Id ?? "week";
        var (from, to) = ResolveDateRange(selectedRange);
        var metrics = dashboardStatisticsService.GetDashboardMetrics(from, to, selectedRange);

        AverageEstimatedCaloriesBurned = metrics.AverageEstimatedCaloriesBurned <= 0
            ? "0 kcal"
            : $"{metrics.AverageEstimatedCaloriesBurned:0} kcal";

        AverageWorkoutTime = FormatMinutes(metrics.AverageWorkoutMinutes);
        AverageSessionsPerWeek = metrics.AverageSessionsPerWeek.ToString("0.#");

        ApplyTargets(metrics.TargetProgress);
        ApplyActivityTimeline(metrics.ActivityTimeline);
        ApplyPreferredMuscleGroups(metrics.PreferredMuscleGroups);
        ApplyPreferredEquipment(metrics.PreferredEquipment);
        ApplyPreferredMechanics(metrics.PreferredMechanics);
    }

    private void ApplyTargets(DashboardTargetProgress targetProgress)
    {
        sessionTargetActual = targetProgress.ActualSessions;
        sessionTargetGoal = targetProgress.TargetSessions;
        calorieTargetActual = targetProgress.ActualCalories;
        calorieTargetGoal = targetProgress.TargetCalories;

        SessionTargetValue = $"{targetProgress.ActualSessions} / {targetProgress.TargetSessions}";
        SessionTargetSubtitle = targetProgress.TargetSessions <= 0
            ? "No session target set"
            : "Sessions completed";

        CalorieTargetValue = $"{targetProgress.ActualCalories:0} / {targetProgress.TargetCalories:0} kcal";
        CalorieTargetSubtitle = targetProgress.TargetCalories <= 0
            ? "No calorie target set"
            : "Calories burned";

        OnPropertyChanged(nameof(SessionTargetProgress));
        OnPropertyChanged(nameof(SessionTargetProgressText));
        OnPropertyChanged(nameof(CalorieTargetProgress));
        OnPropertyChanged(nameof(CalorieTargetProgressText));
    }

    private void ApplyActivityTimeline(IReadOnlyList<DashboardPointMetric> timeline)
    {
        HasActivityTimelineData = timeline.Any(point => point.Value > 0);

        ActivityTimelineSeries =
        [
            new ColumnSeries<double>
            {
                Name = "Completed workouts",
                Values = timeline.Select(point => point.Value).ToArray()
            }
        ];

        ActivityTimelineXAxes =
        [
            new Axis
            {
                Labels = timeline.Select(point => point.Label).ToArray()
            }
        ];

        ActivityTimelineYAxes = [new Axis()];
    }

    private void ApplyPreferredMuscleGroups(IReadOnlyList<DashboardCategoryMetric> muscleGroups)
    {
        HasPreferredMuscleGroupsData = muscleGroups.Any(metric => metric.Value > 0);

        PreferredMuscleGroupsSeries = muscleGroups
            .Where(metric => metric.Value > 0)
            .Select(metric => new PieSeries<double>
            {
                Name = metric.Label,
                Values = [metric.Value]
            })
            .Cast<ISeries>()
            .ToArray();
    }

    private void ApplyPreferredEquipment(IReadOnlyList<DashboardCategoryMetric> equipment)
    {
        HasPreferredEquipmentData = equipment.Any(metric => metric.Value > 0);

        PreferredEquipmentSeries =
        [
            new ColumnSeries<double>
            {
                Name = "Completed sets",
                Values = equipment.Select(metric => metric.Value).ToArray()
            }
        ];

        PreferredEquipmentXAxes =
        [
            new Axis
            {
                Labels = equipment.Select(metric => metric.Label).ToArray()
            }
        ];

        PreferredEquipmentYAxes = [new Axis()];
    }

    private void ApplyPreferredMechanics(IReadOnlyList<DashboardCategoryMetric> mechanics)
    {
        HasPreferredMechanicData = mechanics.Any(metric => metric.Value > 0);

        PreferredMechanicSeries =
        [
            new ColumnSeries<double>
            {
                Name = "Completed sets",
                Values = mechanics.Select(metric => metric.Value).ToArray()
            }
        ];

        PreferredMechanicXAxes =
        [
            new Axis
            {
                Labels = mechanics.Select(metric => metric.Label).ToArray()
            }
        ];

        PreferredMechanicYAxes = [new Axis()];
    }

    private static (DateTime From, DateTime To) ResolveDateRange(string rangeId)
    {
        var today = DateTime.Today;
        var to = today.AddDays(1).AddTicks(-1);

        var from = rangeId switch
        {
            "month" => today.AddMonths(-1),
            "3m" => today.AddMonths(-3),
            "6m" => today.AddMonths(-6),
            "year" => today.AddYears(-1),
            _ => today.AddDays(-6)
        };

        return (from, to);
    }

    private static double CalculateProgress(double current, double target)
    {
        if (target <= 0)
            return 0;

        return Math.Clamp(current / target, 0, 1);
    }

    private static string FormatMinutes(double minutes)
    {
        var roundedMinutes = Math.Max(0, (int)Math.Round(minutes));

        if (roundedMinutes < 60)
            return $"{roundedMinutes}m";

        var hours = roundedMinutes / 60;
        var remainingMinutes = roundedMinutes % 60;

        return remainingMinutes == 0
            ? $"{hours}h"
            : $"{hours}h {remainingMinutes}m";
    }
}

public partial class DashboardRangeTabPresentationModel : ObservableObject
{
    [ObservableProperty]
    public partial string Id { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}