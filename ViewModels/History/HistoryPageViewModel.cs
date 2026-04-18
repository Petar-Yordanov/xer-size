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

    [ObservableProperty]
    public partial bool IsRoutineSheetOpen { get; set; }

    [ObservableProperty]
    public partial bool IsWorkoutSheetOpen { get; set; }

    public string SelectedRoutineName => SelectedRoutine?.Name ?? "Select routine";
    public string SelectedWorkoutName => SelectedWorkout?.Name ?? "Select workout";

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
        foreach (var routine in routines)
            Routines.Add(routine);

        SelectedRoutine ??= Routines.FirstOrDefault();
        RefreshWorkouts();

        _isLoaded = true;
        await BuildRowsAsync();
    }

    [RelayCommand]
    private void ToggleSessionInfo(HistorySessionRow? row)
    {
        if (row is null)
            return;

        row.IsInfoExpanded = !row.IsInfoExpanded;
    }

    [RelayCommand]
    private void ToggleRoutineSheet()
    {
        IsRoutineSheetOpen = !IsRoutineSheetOpen;
        if (IsRoutineSheetOpen)
            IsWorkoutSheetOpen = false;
    }

    [RelayCommand]
    private void ToggleWorkoutSheet()
    {
        IsWorkoutSheetOpen = !IsWorkoutSheetOpen;
        if (IsWorkoutSheetOpen)
            IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    private void CloseRoutineSheet()
    {
        IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    private void CloseWorkoutSheet()
    {
        IsWorkoutSheetOpen = false;
    }

    [RelayCommand]
    private void SelectRoutine(Routine? routine)
    {
        if (routine is null)
            return;

        SelectedRoutine = routine;
        IsRoutineSheetOpen = false;
    }

    [RelayCommand]
    private void SelectWorkout(Workout? workout)
    {
        if (workout is null)
            return;

        SelectedWorkout = workout;
        IsWorkoutSheetOpen = false;
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
            SelectedWorkout = value.Workouts.FirstOrDefault();
        }

        OnPropertyChanged(nameof(SelectedRoutineName));
        OnPropertyChanged(nameof(SelectedWorkoutName));

        _ = BuildRowsAsync();
    }

    partial void OnSelectedWorkoutChanged(Workout? value)
    {
        OnPropertyChanged(nameof(SelectedWorkoutName));
        _ = BuildRowsAsync();
    }

    private void RefreshWorkouts()
    {
        Workouts.Clear();

        if (SelectedRoutine is null)
            return;

        foreach (var workout in SelectedRoutine.Workouts)
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
                        Subtitle = $"{session.Exercises.Count} exercises",

                        DateText = session.PerformedAt.ToString("ddd, dd MMM yyyy • HH:mm"),
                        DurationText = $"{session.DurationMinutes} min",
                        CaloriesText = $"{session.EstimatedCaloriesBurned:F0} kcal",
                        BodyWeightText = $"{session.BodyWeightKg:F1} kg",

                        VolumeText = $"{session.TotalVolume:F0} kg",
                        SetsText = $"{session.TotalSets} sets",
                        RepsText = $"{currentReps} reps",

                        VolumeDeltaText = BuildDeltaText(volumeDelta, "kg"),
                        RepsDeltaText = BuildDeltaText(repsDelta, "reps"),

                        IsVolumeUp = volumeDelta.HasValue && volumeDelta.Value > 0,
                        IsVolumeDown = volumeDelta.HasValue && volumeDelta.Value < 0,
                        IsRepsUp = repsDelta.HasValue && repsDelta.Value > 0,
                        IsRepsDown = repsDelta.HasValue && repsDelta.Value < 0,

                        IsInfoExpanded = false,
                        ExerciseInfos = session.Exercises
                            .OrderBy(x => x.Name)
                            .Select(exercise => new HistorySessionExerciseInfoRow
                            {
                                Name = exercise.Name,
                                SummaryText = BuildExerciseSummary(exercise),
                                Sets = exercise.Sets
                                    .OrderBy(s => s.Order)
                                    .Select(s => new HistorySessionSetInfoRow
                                    {
                                        Text = BuildSetText(s),
                                        Parts = BuildSetParts(s)
                                    })
                                    .ToList()
                            })
                            .ToList()
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

    private static string BuildDeltaText(double? delta, string unit)
    {
        if (!delta.HasValue)
            return "First entry";

        if (delta.Value > 0)
            return $"+{delta.Value:F0} {unit}";

        if (delta.Value < 0)
            return $"{delta.Value:F0} {unit}";

        return $"Same {unit}";
    }

    private static string BuildDeltaText(int? delta, string unit)
    {
        if (!delta.HasValue)
            return "First entry";

        if (delta.Value > 0)
            return $"+{delta.Value} {unit}";

        if (delta.Value < 0)
            return $"{delta.Value} {unit}";

        return $"Same {unit}";
    }

    private static string BuildExerciseSummary(LoggedWorkoutExercise exercise)
    {
        var totalReps = exercise.Sets.Sum(x => x.Reps ?? 0);
        var totalDurationSeconds = exercise.Sets.Sum(x => x.DurationSeconds ?? 0);

        var parts = new List<string>
        {
            $"{exercise.Sets.Count} sets"
        };

        if (totalReps > 0)
            parts.Add($"{totalReps} reps");

        if (exercise.TotalVolume > 0)
            parts.Add($"{exercise.TotalVolume:F0} volume");

        if (totalDurationSeconds > 0)
            parts.Add($"{totalDurationSeconds}s");

        return string.Join(" • ", parts);
    }

    private static string BuildSetText(LoggedSet set)
    {
        return string.Join(" × ", BuildSetPartTexts(set));
    }

    private static IReadOnlyList<SetPartItem> BuildSetParts(LoggedSet set)
    {
        return BuildSetPartTexts(set)
            .Select((text, index) => new SetPartItem
            {
                Text = text,
                ShowSeparator = index > 0
            })
            .ToList();
    }

    private static IReadOnlyList<string> BuildSetPartTexts(LoggedSet set)
    {
        var parts = new List<string> { $"Set {set.Order}" };

        if (set.Reps.HasValue && set.Reps.Value > 0)
            parts.Add($"{set.Reps.Value} reps");

        if (set.WeightKg.HasValue && set.WeightKg.Value > 0)
            parts.Add($"{set.WeightKg.Value:F1} kg");

        if (set.DurationSeconds.HasValue && set.DurationSeconds.Value > 0)
            parts.Add($"{set.DurationSeconds.Value}s");

        if (set.RestSeconds.HasValue && set.RestSeconds.Value > 0)
            parts.Add($"Rest {set.RestSeconds.Value}s");

        return parts;
    }
}