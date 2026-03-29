using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.Statistics;

[QueryProperty(nameof(SeedRoutineIdText), "routineId")]
[QueryProperty(nameof(SeedWorkoutIdText), "workoutId")]
[QueryProperty(nameof(SeedTabText), "tab")]
public partial class StatisticsPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private readonly IStatisticsService _statisticsService;

    private bool _isLoaded;
    private bool _isRefreshing;
    private bool _suppressAutoRefresh;
    private bool _seedApplied;

    [ObservableProperty] public partial string SeedRoutineIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SeedWorkoutIdText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SeedTabText { get; set; } = string.Empty;

    public ObservableCollection<Routine> Routines { get; } = new();
    public ObservableCollection<Workout> Workouts { get; } = new();
    public ObservableCollection<int> AvailableYears { get; } = new();

    [ObservableProperty] public partial Routine? SelectedRoutine { get; set; }
    [ObservableProperty] public partial Workout? SelectedWorkout { get; set; }

    [ObservableProperty] public partial StatisticsTab SelectedTab { get; set; } = StatisticsTab.Preferences;
    [ObservableProperty] public partial StatisticsTimelineBucket SelectedTimelineBucket { get; set; } = StatisticsTimelineBucket.Weeks;
    [ObservableProperty] public partial StatisticsQuarter SelectedQuarter { get; set; } = GetCurrentQuarter();
    [ObservableProperty] public partial int SelectedYear { get; set; }

    [ObservableProperty] public partial StatisticsSnapshot? Snapshot { get; set; }

    [ObservableProperty] public partial bool IsRoutineSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsWorkoutSheetOpen { get; set; }
    [ObservableProperty] public partial bool IsYearSheetOpen { get; set; }

    [ObservableProperty] public partial ISeries[] VolumeSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] VolumeXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] VolumeYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] LiftedWeightSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] LiftedWeightXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] LiftedWeightYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] GymVisitFrequencySeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] GymVisitFrequencyXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] GymVisitFrequencyYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] EquipmentPreferenceSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial ISeries[] MuscleGroupPreferenceSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial ISeries[] MechanicComparisonSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] MechanicComparisonXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] MechanicComparisonYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] ExerciseVolumePreferenceSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] ExerciseVolumePreferenceXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] ExerciseVolumePreferenceYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] ExerciseVolumeTimelineSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] ExerciseVolumeTimelineXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] ExerciseVolumeTimelineYAxes { get; set; } = Array.Empty<Axis>();

    public string SelectedRoutineText => SelectedRoutine?.Name ?? "Select routine";
    public string SelectedWorkoutText => SelectedWorkout?.Name ?? "Select workout";
    public string SelectedYearText => SelectedYear <= 0 ? "Select year" : SelectedYear.ToString();

    public bool HasRoutineSelection => SelectedRoutine is not null;
    public bool HasWorkoutSelection => SelectedWorkout is not null;
    public bool HasAnySelection => SelectedRoutine is not null;

    public bool IsPreferencesTabSelected => SelectedTab == StatisticsTab.Preferences;
    public bool IsTimelineTabSelected => SelectedTab == StatisticsTab.TimelineData;

    public bool IsDaysSelected => SelectedTimelineBucket == StatisticsTimelineBucket.Days;
    public bool IsWeeksSelected => SelectedTimelineBucket == StatisticsTimelineBucket.Weeks;
    public bool IsMonthsSelected => SelectedTimelineBucket == StatisticsTimelineBucket.Months;

    public bool IsQ1Selected => SelectedQuarter == StatisticsQuarter.Q1;
    public bool IsQ2Selected => SelectedQuarter == StatisticsQuarter.Q2;
    public bool IsQ3Selected => SelectedQuarter == StatisticsQuarter.Q3;
    public bool IsQ4Selected => SelectedQuarter == StatisticsQuarter.Q4;

    public StatisticsPageViewModel(IRoutineService routineService, IStatisticsService statisticsService)
    {
        _routineService = routineService;
        _statisticsService = statisticsService;
    }

    partial void OnSeedRoutineIdTextChanged(string value) => _ = ApplySeedIfPossibleAsync();
    partial void OnSeedWorkoutIdTextChanged(string value) => _ = ApplySeedIfPossibleAsync();
    partial void OnSeedTabTextChanged(string value) => _ = ApplySeedIfPossibleAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        var routines = await _routineService.GetAllAsync();

        _suppressAutoRefresh = true;

        Routines.Clear();
        foreach (var routine in routines)
            Routines.Add(routine);

        RefreshWorkoutsInternal();

        _suppressAutoRefresh = false;
        _isLoaded = true;

        RaiseSelectionProperties();

        await ApplySeedIfPossibleAsync();
        await RefreshAvailableYearsAsync();
        await RefreshSnapshotAsync();
    }

    public async Task OnPageAppearingAsync()
    {
        if (!_isLoaded)
            await LoadAsync();
        else
        {
            await ApplySeedIfPossibleAsync();
            await RefreshAvailableYearsAsync();
            await RefreshSnapshotAsync();
        }
    }

    [RelayCommand]
    private void ToggleRoutineSheet()
    {
        IsRoutineSheetOpen = !IsRoutineSheetOpen;
        if (IsRoutineSheetOpen)
        {
            IsWorkoutSheetOpen = false;
            IsYearSheetOpen = false;
        }
    }

    [RelayCommand]
    private void ToggleWorkoutSheet()
    {
        if (SelectedRoutine is null)
            return;

        IsWorkoutSheetOpen = !IsWorkoutSheetOpen;
        if (IsWorkoutSheetOpen)
        {
            IsRoutineSheetOpen = false;
            IsYearSheetOpen = false;
        }
    }

    [RelayCommand]
    private void ToggleYearSheet()
    {
        if (!HasAnySelection)
            return;

        IsYearSheetOpen = !IsYearSheetOpen;
        if (IsYearSheetOpen)
        {
            IsRoutineSheetOpen = false;
            IsWorkoutSheetOpen = false;
        }
    }

    [RelayCommand] private void CloseRoutineSheet() => IsRoutineSheetOpen = false;
    [RelayCommand] private void CloseWorkoutSheet() => IsWorkoutSheetOpen = false;
    [RelayCommand] private void CloseYearSheet() => IsYearSheetOpen = false;

    [RelayCommand]
    private void SelectRoutine(Routine? routine)
    {
        SelectedRoutine = routine;
        IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    private void SelectWorkout(Workout? workout)
    {
        SelectedWorkout = workout;
        IsWorkoutSheetOpen = false;
    }

    [RelayCommand]
    private void SelectYear(int year)
    {
        SelectedYear = year;
        IsYearSheetOpen = false;
    }

    [RelayCommand]
    private void ClearRoutineSelection()
    {
        _suppressAutoRefresh = true;

        SelectedRoutine = null;
        SelectedWorkout = null;
        SelectedYear = 0;
        Workouts.Clear();
        AvailableYears.Clear();

        _suppressAutoRefresh = false;

        IsRoutineSheetOpen = false;
        IsWorkoutSheetOpen = false;
        IsYearSheetOpen = false;

        Snapshot = new StatisticsSnapshot
        {
            HasSelection = false,
            SelectionSummary = "Select a routine to view current setup preferences and logged timeline history."
        };

        BuildCharts(Snapshot);
        RaiseSelectionProperties();
    }

    [RelayCommand]
    private async Task ClearWorkoutSelectionAsync()
    {
        SelectedWorkout = null;
        IsWorkoutSheetOpen = false;
        await RefreshAvailableYearsAsync();
        RaiseSelectionProperties();
        await RefreshSnapshotAsync();
    }

    [RelayCommand] private void ShowPreferencesTab() => SelectedTab = StatisticsTab.Preferences;
    [RelayCommand] private void ShowTimelineTab() => SelectedTab = StatisticsTab.TimelineData;

    [RelayCommand] private void SelectDaysBucket() => SelectedTimelineBucket = StatisticsTimelineBucket.Days;
    [RelayCommand] private void SelectWeeksBucket() => SelectedTimelineBucket = StatisticsTimelineBucket.Weeks;
    [RelayCommand] private void SelectMonthsBucket() => SelectedTimelineBucket = StatisticsTimelineBucket.Months;

    [RelayCommand] private void SelectQ1() => SelectedQuarter = StatisticsQuarter.Q1;
    [RelayCommand] private void SelectQ2() => SelectedQuarter = StatisticsQuarter.Q2;
    [RelayCommand] private void SelectQ3() => SelectedQuarter = StatisticsQuarter.Q3;
    [RelayCommand] private void SelectQ4() => SelectedQuarter = StatisticsQuarter.Q4;

    partial void OnSelectedRoutineChanged(Routine? value)
    {
        if (_suppressAutoRefresh)
        {
            RaiseSelectionProperties();
            return;
        }

        _suppressAutoRefresh = true;
        RefreshWorkoutsInternal();
        _suppressAutoRefresh = false;

        RaiseSelectionProperties();
        _ = HandleSelectionChangedAsync();
    }

    partial void OnSelectedWorkoutChanged(Workout? value)
    {
        RaiseSelectionProperties();

        if (_suppressAutoRefresh)
            return;

        _ = HandleSelectionChangedAsync();
    }

    partial void OnSelectedTabChanged(StatisticsTab value)
    {
        OnPropertyChanged(nameof(IsPreferencesTabSelected));
        OnPropertyChanged(nameof(IsTimelineTabSelected));
    }

    partial void OnSelectedTimelineBucketChanged(StatisticsTimelineBucket value)
    {
        OnPropertyChanged(nameof(IsDaysSelected));
        OnPropertyChanged(nameof(IsWeeksSelected));
        OnPropertyChanged(nameof(IsMonthsSelected));

        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    partial void OnSelectedYearChanged(int value)
    {
        OnPropertyChanged(nameof(SelectedYearText));

        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    partial void OnSelectedQuarterChanged(StatisticsQuarter value)
    {
        OnPropertyChanged(nameof(IsQ1Selected));
        OnPropertyChanged(nameof(IsQ2Selected));
        OnPropertyChanged(nameof(IsQ3Selected));
        OnPropertyChanged(nameof(IsQ4Selected));

        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    private async Task HandleSelectionChangedAsync()
    {
        await RefreshAvailableYearsAsync();
        await RefreshSnapshotAsync();
    }

    private async Task RefreshAvailableYearsAsync()
    {
        AvailableYears.Clear();

        if (SelectedRoutine is null)
        {
            SelectedYear = 0;
            return;
        }

        var years = await _statisticsService.GetAvailableTimelineYearsAsync(
            SelectedRoutine.Id,
            SelectedWorkout?.Id);

        foreach (var year in years)
            AvailableYears.Add(year);

        if (SelectedYear == 0 || !AvailableYears.Contains(SelectedYear))
            SelectedYear = AvailableYears.FirstOrDefault();
    }

    private async Task ApplySeedIfPossibleAsync()
    {
        if (!_isLoaded || _seedApplied)
            return;

        if (!Guid.TryParse(SeedRoutineIdText, out var routineId))
            return;

        _suppressAutoRefresh = true;

        SelectedRoutine = Routines.FirstOrDefault(x => x.Id == routineId);
        RefreshWorkoutsInternal();

        if (Guid.TryParse(SeedWorkoutIdText, out var workoutId))
            SelectedWorkout = Workouts.FirstOrDefault(x => x.Id == workoutId);

        SelectedTab = string.Equals(SeedTabText, "timeline", StringComparison.OrdinalIgnoreCase)
            ? StatisticsTab.TimelineData
            : StatisticsTab.Preferences;

        _suppressAutoRefresh = false;
        _seedApplied = true;

        RaiseSelectionProperties();
    }

    [RelayCommand]
    public async Task RefreshSnapshotAsync()
    {
        if (_isRefreshing)
            return;

        try
        {
            _isRefreshing = true;

            if (SelectedRoutine is null || SelectedYear <= 0)
            {
                var empty = new StatisticsSnapshot
                {
                    HasSelection = false,
                    SelectionSummary = "Select a routine to view current setup preferences and logged timeline history."
                };

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Snapshot = empty;
                    BuildCharts(empty);
                });

                return;
            }

            var routineId = SelectedRoutine.Id;
            var workoutId = SelectedWorkout?.Id;
            var bucket = SelectedTimelineBucket;
            var year = SelectedYear;
            var quarter = SelectedQuarter;

            var snapshot = await Task.Run(async () =>
                await _statisticsService.GetSnapshotAsync(routineId, workoutId, bucket, year, quarter));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Snapshot = snapshot;
                BuildCharts(snapshot);
            });
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void RefreshWorkoutsInternal()
    {
        Workouts.Clear();

        if (SelectedRoutine is null)
        {
            SelectedWorkout = null;
            return;
        }

        foreach (var workout in SelectedRoutine.Workouts)
            Workouts.Add(workout);

        if (SelectedWorkout is not null && Workouts.All(x => x.Id != SelectedWorkout.Id))
            SelectedWorkout = null;
    }

    private void BuildCharts(StatisticsSnapshot snapshot)
    {
        BuildPreferenceCharts(snapshot);
        BuildTimelineCharts(snapshot);
    }

    private void BuildPreferenceCharts(StatisticsSnapshot snapshot)
    {
        EquipmentPreferenceSeries = BuildPieSeries(snapshot.EquipmentPreference.Take(6).ToList());
        MuscleGroupPreferenceSeries = BuildPieSeries(snapshot.MuscleGroupPreference.Take(6).ToList());

        var mechanicLabels = snapshot.MechanicPreference.Select(x => x.Label).ToArray();
        var mechanicValues = snapshot.MechanicPreference.Select(x => x.Value).ToArray();

        MechanicComparisonSeries =
        [
            new ColumnSeries<double>
        {
            Values = mechanicValues,
            Name = "Sets"
        }
        ];

        MechanicComparisonXAxes =
        [
            new Axis
        {
            Labels = mechanicLabels,
            Name = "Mechanic Type"
        }
        ];

        MechanicComparisonYAxes =
        [
            new Axis
        {
            Name = "Sets"
        }
        ];

        var exerciseLabels = snapshot.ExerciseVolumePreference.Select(x => x.Label).ToArray();
        var exerciseValues = snapshot.ExerciseVolumePreference.Select(x => x.Value).ToArray();

        ExerciseVolumePreferenceSeries =
        [
            new ColumnSeries<double>
        {
            Values = exerciseValues,
            Name = "Volume (kg)"
        }
        ];

        ExerciseVolumePreferenceXAxes =
        [
            new Axis
        {
            Labels = exerciseLabels,
            Name = "Exercise"
        }
        ];

        ExerciseVolumePreferenceYAxes =
        [
            new Axis
        {
            Name = "Volume (kg)"
        }
        ];
    }

    private void BuildTimelineCharts(StatisticsSnapshot snapshot)
    {
        BuildOverallTimelineChart(
            snapshot.VolumeTrend,
            "Volume (kg)",
            "Time",
            "Volume (kg)",
            isWeeklyGroupedBars: SelectedTimelineBucket == StatisticsTimelineBucket.Weeks,
            out var volumeSeries,
            out var volumeXAxes,
            out var volumeYAxes);

        VolumeSeries = volumeSeries;
        VolumeXAxes = volumeXAxes;
        VolumeYAxes = volumeYAxes;

        BuildOverallTimelineChart(
            snapshot.LiftedWeightTrend,
            "Average Set Weight (kg)",
            "Time",
            "Average Set Weight (kg)",
            isWeeklyGroupedBars: SelectedTimelineBucket == StatisticsTimelineBucket.Weeks,
            out var liftedSeries,
            out var liftedXAxes,
            out var liftedYAxes);

        LiftedWeightSeries = liftedSeries;
        LiftedWeightXAxes = liftedXAxes;
        LiftedWeightYAxes = liftedYAxes;

        BuildOverallTimelineChart(
            snapshot.GymVisitFrequencyTrend,
            "Visits",
            "Time",
            "Workout Sessions",
            isWeeklyGroupedBars: SelectedTimelineBucket == StatisticsTimelineBucket.Weeks,
            out var visitSeries,
            out var visitXAxes,
            out var visitYAxes);

        GymVisitFrequencySeries = visitSeries;
        GymVisitFrequencyXAxes = visitXAxes;
        GymVisitFrequencyYAxes = visitYAxes;

        BuildExerciseVolumeTimelineChart(snapshot);
    }

    private void BuildOverallTimelineChart(
        IReadOnlyList<TrendPoint> points,
        string seriesName,
        string xAxisName,
        string yAxisName,
        bool isWeeklyGroupedBars,
        out ISeries[] series,
        out Axis[] xAxes,
        out Axis[] yAxes)
    {
        if (!isWeeklyGroupedBars)
        {
            var pointLabels = points.Select(x => x.Label).ToArray();
            var values = points.Select(x => x.Value).ToArray();

            series =
            [
                SelectedTimelineBucket == StatisticsTimelineBucket.Months
                ? new ColumnSeries<double>
                {
                    Values = values,
                    Name = seriesName
                }
                : new LineSeries<double>
                {
                    Values = values,
                    Name = seriesName
                }
            ];

            xAxes =
            [
                new Axis
            {
                Labels = pointLabels,
                Name = xAxisName
            }
            ];

            yAxes =
            [
                new Axis
            {
                Name = yAxisName
            }
            ];
            return;
        }

        var monthGroups = points
            .GroupBy(x => x.Date.Month)
            .OrderBy(x => x.Key)
            .ToList();

        var maxWeek = Math.Max(
            1,
            points.Select(x => (((x.Date.Day - 1) / 7) + 1)).DefaultIfEmpty(1).Max());

        var weekLabels = Enumerable.Range(1, maxWeek)
            .Select(x => $"W{x}")
            .ToArray();

        series = monthGroups
            .Select(group =>
            {
                var values = new double[maxWeek];

                foreach (var point in group)
                {
                    var weekIndex = ((point.Date.Day - 1) / 7);
                    if (weekIndex >= 0 && weekIndex < maxWeek)
                        values[weekIndex] = point.Value;
                }

                return (ISeries)new ColumnSeries<double>
                {
                    Values = values,
                    Name = new DateTime(SelectedYear, group.Key, 1).ToString("MMM")
                };
            })
            .ToArray();

        xAxes =
        [
            new Axis
        {
            Labels = weekLabels,
            Name = "Week of Month"
        }
        ];

        yAxes =
        [
            new Axis
        {
            Name = yAxisName
        }
        ];
    }

    private void BuildExerciseVolumeTimelineChart(StatisticsSnapshot snapshot)
    {
        var allLabels = snapshot.ExerciseVolumeTrendSeries
            .FirstOrDefault()?.Points
            .Select(x => SelectedTimelineBucket switch
            {
                StatisticsTimelineBucket.Weeks => $"{x.Date:MMM} W{(((x.Date.Day - 1) / 7) + 1)}",
                _ => x.Label
            })
            .ToArray() ?? Array.Empty<string>();

        ExerciseVolumeTimelineSeries = snapshot.ExerciseVolumeTrendSeries
            .Select(series => (ISeries)new ColumnSeries<double>
            {
                Values = series.Points.Select(x => x.Value).ToArray(),
                Name = series.Label
            })
            .ToArray();

        ExerciseVolumeTimelineXAxes =
        [
            new Axis
        {
            Labels = allLabels,
            Name = SelectedTimelineBucket == StatisticsTimelineBucket.Weeks
                ? "Week / Month"
                : "Time"
        }
        ];

        ExerciseVolumeTimelineYAxes =
        [
            new Axis
        {
            Name = "Volume (kg)"
        }
        ];
    }

    private static ISeries[] BuildPieSeries(IReadOnlyList<BreakdownItem> items)
    {
        if (items.Count == 0)
            return Array.Empty<ISeries>();

        return items
            .Select(item => (ISeries)new PieSeries<double>
            {
                Values = [item.Value],
                Name = item.Label
            })
            .ToArray();
    }

    private void RaiseSelectionProperties()
    {
        OnPropertyChanged(nameof(SelectedRoutineText));
        OnPropertyChanged(nameof(SelectedWorkoutText));
        OnPropertyChanged(nameof(SelectedYearText));
        OnPropertyChanged(nameof(HasRoutineSelection));
        OnPropertyChanged(nameof(HasWorkoutSelection));
        OnPropertyChanged(nameof(HasAnySelection));
    }

    private static StatisticsQuarter GetCurrentQuarter()
    {
        var month = DateTime.Today.Month;
        return month switch
        {
            <= 3 => StatisticsQuarter.Q1,
            <= 6 => StatisticsQuarter.Q2,
            <= 9 => StatisticsQuarter.Q3,
            _ => StatisticsQuarter.Q4
        };
    }
}