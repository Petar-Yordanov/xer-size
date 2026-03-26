using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.Statistics;

public partial class StatisticsPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private readonly IStatisticsService _statisticsService;

    private bool _isLoaded;
    private bool _isRefreshing;
    private bool _suppressAutoRefresh;

    public IReadOnlyList<StatisticsRange> AvailableRanges { get; } = Enum.GetValues<StatisticsRange>();
    public IReadOnlyList<StatisticsScopeKind> AvailableScopes { get; } = Enum.GetValues<StatisticsScopeKind>();

    public ObservableCollection<Routine> Routines { get; } = new();
    public ObservableCollection<Workout> Workouts { get; } = new();

    [ObservableProperty] public partial StatisticsRange SelectedRange { get; set; } = StatisticsRange.LastMonth;
    [ObservableProperty] public partial StatisticsScopeKind SelectedScope { get; set; } = StatisticsScopeKind.Routine;
    [ObservableProperty] public partial Routine? SelectedRoutine { get; set; }
    [ObservableProperty] public partial Workout? SelectedWorkout { get; set; }
    [ObservableProperty] public partial StatisticsSnapshot? Snapshot { get; set; }

    [ObservableProperty] public partial ISeries[] VolumeSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] VolumeXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] VolumeYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty] public partial ISeries[] WeightSeries { get; set; } = Array.Empty<ISeries>();
    [ObservableProperty] public partial Axis[] WeightXAxes { get; set; } = Array.Empty<Axis>();
    [ObservableProperty] public partial Axis[] WeightYAxes { get; set; } = Array.Empty<Axis>();

    public StatisticsPageViewModel(IRoutineService routineService, IStatisticsService statisticsService)
    {
        _routineService = routineService;
        _statisticsService = statisticsService;
    }

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

        SelectedRoutine ??= Routines.FirstOrDefault();
        RefreshWorkoutsInternal();

        _suppressAutoRefresh = false;
        _isLoaded = true;

        await RefreshSnapshotAsync();
    }

    partial void OnSelectedRoutineChanged(Routine? value)
    {
        if (_suppressAutoRefresh)
            return;

        _suppressAutoRefresh = true;
        RefreshWorkoutsInternal();
        _suppressAutoRefresh = false;

        _ = RefreshSnapshotAsync();
    }

    partial void OnSelectedWorkoutChanged(Workout? value)
    {
        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    partial void OnSelectedRangeChanged(StatisticsRange value)
    {
        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    partial void OnSelectedScopeChanged(StatisticsScopeKind value)
    {
        if (_suppressAutoRefresh)
            return;

        _ = RefreshSnapshotAsync();
    }

    [RelayCommand]
    private async Task RefreshSnapshotAsync()
    {
        if (_isRefreshing || SelectedRoutine is null)
            return;

        try
        {
            _isRefreshing = true;

            var snapshot = await _statisticsService.GetSnapshotAsync(
                SelectedRange,
                SelectedScope,
                SelectedRoutine.Id,
                SelectedScope == StatisticsScopeKind.Workout ? SelectedWorkout?.Id : null);

            Snapshot = snapshot;

            await Task.Delay(50);
            await MainThread.InvokeOnMainThreadAsync(() => BuildCharts(snapshot));
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

        foreach (var workout in SelectedRoutine.Workouts.OrderBy(x => x.Name))
            Workouts.Add(workout);

        if (SelectedScope == StatisticsScopeKind.Workout)
            SelectedWorkout = Workouts.FirstOrDefault();
        else if (SelectedWorkout is not null && Workouts.All(x => x.Id != SelectedWorkout.Id))
            SelectedWorkout = Workouts.FirstOrDefault();
    }

    private void BuildCharts(StatisticsSnapshot snapshot)
    {
        var volumeLabels = snapshot.VolumeTrend.Select(x => x.Label).ToArray();
        var volumeValues = snapshot.VolumeTrend.Select(x => x.Value).ToArray();

        VolumeSeries =
        [
            new LineSeries<double> { Values = volumeValues }
        ];
        VolumeXAxes =
        [
            new Axis { Labels = volumeLabels }
        ];
        VolumeYAxes =
        [
            new Axis()
        ];

        var weightLabels = snapshot.WeightTrend.Select(x => x.Label).ToArray();
        var weightValues = snapshot.WeightTrend.Select(x => x.Value).ToArray();

        WeightSeries =
        [
            new LineSeries<double> { Values = weightValues }
        ];
        WeightXAxes =
        [
            new Axis { Labels = weightLabels }
        ];
        WeightYAxes =
        [
            new Axis()
        ];
    }
}