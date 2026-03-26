using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.History;

public partial class HistoryPageViewModel : ObservableObject
{
    private readonly IRoutineService _routineService;
    private readonly IWorkoutHistoryService _historyService;

    private List<LoggedWorkoutSession> _allSessions = new();
    private bool _isLoaded;
    private bool _isBuilding;

    public ObservableCollection<Routine> Routines { get; } = new();
    public ObservableCollection<Workout> Workouts { get; } = new();
    public ObservableCollection<HistorySessionRow> Sessions { get; } = new();

    [ObservableProperty]
    public partial Routine? SelectedRoutine { get; set; }

    [ObservableProperty]
    public partial Workout? SelectedWorkout { get; set; }

    public HistoryPageViewModel(IRoutineService routineService, IWorkoutHistoryService historyService)
    {
        _routineService = routineService;
        _historyService = historyService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        var routines = await _routineService.GetAllAsync();
        _allSessions = (await _historyService.GetAllAsync())
            .OrderBy(x => x.PerformedAt)
            .ToList();

        Routines.Clear();
        foreach (var routine in routines.OrderBy(x => x.Name))
            Routines.Add(routine);

        SelectedRoutine ??= Routines.FirstOrDefault();
        RefreshWorkouts();

        _isLoaded = true;
        await BuildRowsAsync();
    }

    [RelayCommand]
    private Task OpenSessionAsync(HistorySessionRow? row)
    {
        if (row is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync($"{nameof(Views.Pages.HistorySessionDetailsPage)}?sessionId={row.SessionId}");
    }

    partial void OnSelectedRoutineChanged(Routine? value)
    {
        RefreshWorkouts();

        if (value is null)
        {
            SelectedWorkout = null;
        }
        else if (SelectedWorkout is null || value.Workouts.All(x => x.Id != SelectedWorkout.Id))
        {
            SelectedWorkout = value.Workouts.OrderBy(x => x.Name).FirstOrDefault();
        }

        _ = BuildRowsAsync();
    }

    partial void OnSelectedWorkoutChanged(Workout? value)
    {
        _ = BuildRowsAsync();
    }

    private void RefreshWorkouts()
    {
        Workouts.Clear();

        if (SelectedRoutine is null)
            return;

        foreach (var workout in SelectedRoutine.Workouts.OrderBy(x => x.Name))
            Workouts.Add(workout);
    }

    private async Task BuildRowsAsync()
    {
        if (_isBuilding)
            return;

        try
        {
            _isBuilding = true;

            var selectedRoutineId = SelectedRoutine?.Id;
            var selectedWorkoutId = SelectedWorkout?.Id;
            var allSessions = _allSessions;

            var rows = await Task.Run(() =>
            {
                var filtered = allSessions
                    .Where(x => !selectedRoutineId.HasValue || x.RoutineId == selectedRoutineId.Value)
                    .Where(x => !selectedWorkoutId.HasValue || x.WorkoutId == selectedWorkoutId.Value)
                    .OrderByDescending(x => x.PerformedAt)
                    .ToList();

                return filtered.Select(session =>
                {
                    var previous = GetPreviousComparableSession(session, allSessions);

                    var currentReps = GetTotalReps(session);
                    var previousReps = previous is null ? (int?)null : GetTotalReps(previous);

                    var volumeDelta = previous is null ? (double?)null : session.TotalVolume - previous.TotalVolume;
                    var repsDelta = previous is null ? (int?)null : currentReps - previousReps!.Value;

                    return new HistorySessionRow
                    {
                        SessionId = session.Id,
                        Title = $"{session.RoutineName} • {session.WorkoutName}",
                        Subtitle = "Tap for exercise details",

                        DateText = session.PerformedAt.ToString("ddd, dd MMM yyyy • HH:mm"),
                        DurationText = $"{session.DurationMinutes} min",
                        CaloriesText = $"{session.EstimatedCaloriesBurned:F0} kcal",
                        BodyWeightText = $"{session.BodyWeightKg:F1} kg",

                        VolumeText = $"{session.TotalVolume:F0} kg",
                        SetsText = $"{session.TotalSets} sets",
                        RepsText = $"{currentReps} reps",

                        VolumeDeltaText = BuildDeltaText(volumeDelta, "kg", "volume"),
                        RepsDeltaText = BuildDeltaText(repsDelta, "reps", "reps"),

                        IsVolumeUp = volumeDelta.HasValue && volumeDelta.Value > 0,
                        IsVolumeDown = volumeDelta.HasValue && volumeDelta.Value < 0,
                        IsRepsUp = repsDelta.HasValue && repsDelta.Value > 0,
                        IsRepsDown = repsDelta.HasValue && repsDelta.Value < 0
                    };
                }).ToList();
            });

            Sessions.Clear();
            foreach (var row in rows)
                Sessions.Add(row);
        }
        finally
        {
            _isBuilding = false;
        }
    }

    private static LoggedWorkoutSession? GetPreviousComparableSession(
        LoggedWorkoutSession current,
        IReadOnlyList<LoggedWorkoutSession> allSessions)
    {
        return allSessions
            .Where(x => x.RoutineId == current.RoutineId)
            .Where(x => x.WorkoutId == current.WorkoutId)
            .Where(x => x.PerformedAt < current.PerformedAt)
            .OrderByDescending(x => x.PerformedAt)
            .FirstOrDefault();
    }

    private static int GetTotalReps(LoggedWorkoutSession session)
        => session.Exercises.Sum(x => x.Sets.Sum(s => s.Reps ?? 0));

    private static string BuildDeltaText(double? delta, string unit, string label)
    {
        if (!delta.HasValue)
            return $"First logged {label}";

        if (delta.Value > 0)
            return $"▲ +{delta.Value:F0} {unit} vs last";

        if (delta.Value < 0)
            return $"▼ {delta.Value:F0} {unit} vs last";

        return $"= Same {label} as last";
    }

    private static string BuildDeltaText(int? delta, string unit, string label)
    {
        if (!delta.HasValue)
            return $"First logged {label}";

        if (delta.Value > 0)
            return $"▲ +{delta.Value} {unit} vs last";

        if (delta.Value < 0)
            return $"▼ {delta.Value} {unit} vs last";

        return $"= Same {label} as last";
    }
}