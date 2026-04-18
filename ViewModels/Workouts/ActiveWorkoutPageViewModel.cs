using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.Workouts;

public partial class ActiveWorkoutPageViewModel : ObservableObject
{
    private readonly IActiveWorkoutService _activeWorkoutService;
    private readonly IWorkoutHistoryService _historyService;

    private IDispatcherTimer? _timer;
    private ActiveWorkoutSession? _session;
    private bool _isLoaded;
    private bool _isBusy;

    public ObservableCollection<ActiveWorkoutBreadcrumbRow> Breadcrumbs { get; } = new();
    public ObservableCollection<ActiveWorkoutSetRow> Sets { get; } = new();

    [ObservableProperty]
    public partial string RoutineName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WorkoutName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ElapsedText { get; set; } = "00:00:00";

    [ObservableProperty]
    public partial string StartedAtText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentExerciseName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentExerciseMeta { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentExerciseNotes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentExerciseProgressText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RestCountdownText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsRestActive { get; set; }

    [ObservableProperty]
    public partial bool CanGoPrevious { get; set; }

    [ObservableProperty]
    public partial bool CanGoNext { get; set; }

    public ActiveWorkoutPageViewModel(
        IActiveWorkoutService activeWorkoutService,
        IWorkoutHistoryService historyService)
    {
        _activeWorkoutService = activeWorkoutService;
        _historyService = historyService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoaded)
        {
            await RefreshAsync();
            return;
        }

        _session = await _activeWorkoutService.GetCurrentAsync();
        if (_session is null)
            return;

        StartTimer();
        _isLoaded = true;
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SelectExerciseAsync(ActiveWorkoutBreadcrumbRow? row)
    {
        if (_session is null || row is null)
            return;

        await _activeWorkoutService.SelectExerciseAsync(_session.Id, row.Index);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task GoPreviousAsync()
    {
        if (_session is null)
            return;

        var previousIndex = Math.Max(0, _session.CurrentExerciseIndex - 1);
        await _activeWorkoutService.SelectExerciseAsync(_session.Id, previousIndex);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task GoNextAsync()
    {
        if (_session is null)
            return;

        var nextIndex = Math.Min(_session.Exercises.Count - 1, _session.CurrentExerciseIndex + 1);
        await _activeWorkoutService.SelectExerciseAsync(_session.Id, nextIndex);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleSetStateAsync(ActiveWorkoutSetRow? row)
    {
        if (_session is null || row is null)
            return;

        if (row.IsCompleted || row.IsSkipped)
            return;

        if (!row.IsStarted)
        {
            await _activeWorkoutService.StartSetAsync(_session.Id, _session.CurrentExerciseIndex, row.Order);
        }
        else
        {
            await _activeWorkoutService.CompleteSetAsync(
                _session.Id,
                _session.CurrentExerciseIndex,
                row.Order,
                ParseInt(row.ActualRepsText),
                ParseDouble(row.ActualWeightKgText),
                ParseInt(row.ActualDurationSecondsText));
        }

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SkipSetAsync(ActiveWorkoutSetRow? row)
    {
        if (_session is null || row is null)
            return;

        await _activeWorkoutService.SkipSetAsync(_session.Id, _session.CurrentExerciseIndex, row.Order);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SkipRestAsync()
    {
        if (_session is null)
            return;

        await _activeWorkoutService.SkipRestAsync(_session.Id);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task FinishWorkoutAsync()
    {
        if (_session is null || _isBusy)
            return;

        try
        {
            _isBusy = true;

            var logged = await _activeWorkoutService.FinishAsync(_session.Id);
            await _historyService.AddSessionAsync(logged);

            StopTimer();
            _session = null;
            _isLoaded = false;

            await Shell.Current.GoToAsync("//history");
        }
        finally
        {
            _isBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelWorkoutAsync()
    {
        if (_session is null || _isBusy)
            return;

        var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var confirm = await page.DisplayAlertAsync(
            "Cancel Workout",
            "Cancel the active workout? Progress will be discarded.",
            "Cancel Workout",
            "Keep Going");

        if (!confirm)
            return;

        try
        {
            _isBusy = true;

            await _activeWorkoutService.CancelAsync(_session.Id);
            StopTimer();
            _session = null;
            _isLoaded = false;

            await Shell.Current.GoToAsync("//routines");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        _session = await _activeWorkoutService.GetCurrentAsync();
        if (_session is null)
            return;

        RoutineName = _session.RoutineName;
        WorkoutName = _session.WorkoutName;
        StartedAtText = $"Started {_session.StartedAtUtc.ToLocalTime():HH:mm}";

        UpdateTimeTexts();
        RebuildBreadcrumbs();
        RebuildCurrentExercise();
    }

    private void RebuildBreadcrumbs()
    {
        Breadcrumbs.Clear();

        if (_session is null)
            return;

        for (int i = 0; i < _session.Exercises.Count; i++)
        {
            var exercise = _session.Exercises[i];
            var completedCount = exercise.Sets.Count(x => x.State == SetExecutionState.Completed);
            var skippedCount = exercise.Sets.Count(x => x.State == SetExecutionState.Skipped);
            var doneCount = completedCount + skippedCount;

            Breadcrumbs.Add(new ActiveWorkoutBreadcrumbRow
            {
                Index = i,
                Name = exercise.Name,
                ProgressText = $"{doneCount}/{exercise.Sets.Count}",
                IsCurrent = i == _session.CurrentExerciseIndex,
                IsCompleted = exercise.State == ExerciseExecutionState.Completed,
                IsSkipped = exercise.State == ExerciseExecutionState.Skipped,
                IsUpcoming = i > _session.CurrentExerciseIndex
                    && exercise.State is not ExerciseExecutionState.Completed
                    && exercise.State is not ExerciseExecutionState.Skipped
            });
        }

        CanGoPrevious = _session.CurrentExerciseIndex > 0;
        CanGoNext = _session.CurrentExerciseIndex < _session.Exercises.Count - 1;
    }

    private void RebuildCurrentExercise()
    {
        Sets.Clear();

        if (_session is null || _session.Exercises.Count == 0)
            return;

        var exercise = _session.Exercises[_session.CurrentExerciseIndex];

        CurrentExerciseName = exercise.Name;

        var metaParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(exercise.BodyCategory))
            metaParts.Add(exercise.BodyCategory);
        if (!string.IsNullOrWhiteSpace(exercise.Equipment))
            metaParts.Add(exercise.Equipment);
        if (!string.IsNullOrWhiteSpace(exercise.Mechanic))
            metaParts.Add(exercise.Mechanic);

        CurrentExerciseMeta = string.Join(" • ", metaParts);
        CurrentExerciseNotes = string.IsNullOrWhiteSpace(exercise.Notes)
            ? "No notes for this exercise."
            : exercise.Notes!;

        var completedSets = exercise.Sets.Count(x => x.State == SetExecutionState.Completed);
        var skippedSets = exercise.Sets.Count(x => x.State == SetExecutionState.Skipped);
        CurrentExerciseProgressText = $"Done {completedSets} • Skipped {skippedSets} • Total {exercise.Sets.Count}";

        foreach (var set in exercise.Sets.OrderBy(x => x.Order))
        {
            Sets.Add(new ActiveWorkoutSetRow
            {
                Order = set.Order,
                PlannedRepsText = set.PlannedReps?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                PlannedWeightKgText = set.PlannedWeightKg?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
                PlannedDurationSecondsText = set.PlannedDurationSeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                PlannedRestSecondsText = set.PlannedRestSeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,

                ActualRepsText = set.ActualReps?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                ActualWeightKgText = set.ActualWeightKg?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
                ActualDurationSecondsText = set.ActualDurationSeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,

                IsStarted = set.State == SetExecutionState.Started,
                IsCompleted = set.State == SetExecutionState.Completed,
                IsSkipped = set.State == SetExecutionState.Skipped,
                IsCurrent = set.State is SetExecutionState.NotStarted or SetExecutionState.Started
                            && exercise.Sets
                                .Where(x => x.State is not SetExecutionState.Completed and not SetExecutionState.Skipped)
                                .OrderBy(x => x.Order)
                                .FirstOrDefault()?.Order == set.Order
            });
        }
    }

    private void StartTimer()
    {
        if (_timer is not null)
            return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
            return;

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) => UpdateTimeTexts();
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is null)
            return;

        _timer.Stop();
        _timer = null;
    }

    private void UpdateTimeTexts()
    {
        if (_session is null)
            return;

        var elapsed = DateTime.UtcNow - _session.StartedAtUtc;
        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;

        ElapsedText = elapsed.ToString(@"hh\:mm\:ss");

        if (_session.RestEndsAtUtc.HasValue)
        {
            var remaining = _session.RestEndsAtUtc.Value - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                RestCountdownText = "Rest done";
                IsRestActive = false;
            }
            else
            {
                RestCountdownText = $"Rest {remaining:mm\\:ss}";
                IsRestActive = true;
            }
        }
        else
        {
            RestCountdownText = string.Empty;
            IsRestActive = false;
        }
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static double? ParseDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}

public partial class ActiveWorkoutBreadcrumbRow : ObservableObject
{
    [ObservableProperty] public partial int Index { get; set; }
    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string ProgressText { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsCurrent { get; set; }
    [ObservableProperty] public partial bool IsCompleted { get; set; }
    [ObservableProperty] public partial bool IsSkipped { get; set; }
    [ObservableProperty] public partial bool IsUpcoming { get; set; }
}

public partial class ActiveWorkoutSetRow : ObservableObject
{
    [ObservableProperty] public partial int Order { get; set; }

    [ObservableProperty] public partial string PlannedRepsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PlannedWeightKgText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PlannedDurationSecondsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PlannedRestSecondsText { get; set; } = string.Empty;

    [ObservableProperty] public partial string ActualRepsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string ActualWeightKgText { get; set; } = string.Empty;
    [ObservableProperty] public partial string ActualDurationSecondsText { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsStarted { get; set; }
    [ObservableProperty] public partial bool IsCompleted { get; set; }
    [ObservableProperty] public partial bool IsSkipped { get; set; }
    [ObservableProperty] public partial bool IsCurrent { get; set; }

    public string SetLabel => $"Set {Order}";
    public bool IsDurationSet => !string.IsNullOrWhiteSpace(PlannedDurationSecondsText);
    public string ActionText => IsCompleted ? "Done" : IsStarted ? "Complete" : "Start";

    partial void OnOrderChanged(int value) => OnPropertyChanged(nameof(SetLabel));
    partial void OnIsStartedChanged(bool value) => OnPropertyChanged(nameof(ActionText));
    partial void OnIsCompletedChanged(bool value) => OnPropertyChanged(nameof(ActionText));
}