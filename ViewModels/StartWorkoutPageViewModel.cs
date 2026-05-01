using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.Globalization;
using XerSize.Models.DataAccessObjects.ActiveWorkout;
using XerSize.Models.DataAccessObjects.History;
using XerSize.Models.Presentation.ActiveWorkouts;
using XerSize.Models.Presentation.ExerciseMetadata;
using XerSize.Models.Presentation.History;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class StartWorkoutPageViewModel : ObservableObject
{
    private readonly ActiveWorkoutService activeWorkoutService;
    private readonly WorkoutSelectionService workoutSelectionService;
    private readonly UserSettingsService userSettingsService;
    private readonly WorkoutHistoryService workoutHistoryService;
    private readonly IAudioManager audioManager;

    private IDispatcherTimer? elapsedTimer;
    private IDispatcherTimer? restTimer;
    private IAudioPlayer? restFinishedPlayer;
    private ActiveWorkoutSessionModel? session;

    public StartWorkoutPageViewModel(
        ActiveWorkoutService activeWorkoutService,
        WorkoutSelectionService workoutSelectionService,
        UserSettingsService userSettingsService,
        WorkoutHistoryService workoutHistoryService,
        IAudioManager audioManager)
    {
        this.activeWorkoutService = activeWorkoutService;
        this.workoutSelectionService = workoutSelectionService;
        this.userSettingsService = userSettingsService;
        this.workoutHistoryService = workoutHistoryService;
        this.audioManager = audioManager;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentSet))]
    [NotifyPropertyChangedFor(nameof(HasCurrentExercise))]
    [NotifyPropertyChangedFor(nameof(HasNoCurrentExercise))]
    [NotifyPropertyChangedFor(nameof(HasCurrentSet))]
    public partial ActiveWorkoutExercisePresentationModel? CurrentExercise { get; set; }

    [ObservableProperty]
    public partial string ElapsedText { get; set; } = "00:00";

    [ObservableProperty]
    public partial string RestTimerText { get; set; } = "Ready";

    [ObservableProperty]
    public partial string PrimaryButtonText { get; set; } = "Done";

    [ObservableProperty]
    public partial bool CanCompleteWorkout { get; set; }

    [ObservableProperty]
    public partial bool IsLeaveWorkoutDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsCompletePartialWorkoutDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsInfoDialogVisible { get; set; }

    [ObservableProperty]
    public partial string InfoDialogTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoDialogMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsWorkoutCompleteDialogVisible { get; set; }

    [ObservableProperty]
    public partial bool IsExerciseHistoryDialogVisible { get; set; }

    [ObservableProperty]
    public partial string ExerciseHistoryDialogTitle { get; set; } = "Exercise history";

    [ObservableProperty]
    public partial string ExerciseHistoryDialogMessage { get; set; } = "Previous logged sets for this exercise.";

    public ObservableCollection<ActiveWorkoutExercisePresentationModel> Exercises { get; } = [];

    public ObservableCollection<HistoryExercisePresentationModel> SelectedExerciseHistoryItems { get; } = [];

    public ActiveWorkoutSetPresentationModel? CurrentSet =>
        CurrentExercise?.Sets.FirstOrDefault(set => !set.IsCompleted && !set.IsSkipped);

    public bool HasCurrentExercise => CurrentExercise is not null;

    public bool HasNoCurrentExercise => CurrentExercise is null;

    public bool HasCurrentSet => CurrentSet is not null;

    public bool HasExercises => Exercises.Count > 0;

    public bool IsWorkoutEmpty => !HasExercises;

    public bool HasSelectedExerciseHistory => SelectedExerciseHistoryItems.Count > 0;

    public bool HasNoSelectedExerciseHistory => !HasSelectedExerciseHistory;

    public bool HasWorkoutProgress =>
        Exercises.SelectMany(exercise => exercise.Sets).Any(set => set.IsCompleted || set.IsSkipped);

    public bool HasPartialProgress =>
        Exercises.SelectMany(exercise => exercise.Sets).Any(set => set.IsSkipped) ||
        Exercises.SelectMany(exercise => exercise.Sets).Any(set => !set.IsCompleted && !set.IsSkipped);

    public bool IsResting
    {
        get => session?.IsResting == true;
        private set
        {
            if (session is null || session.IsResting == value)
                return;

            session.IsResting = value;
            OnPropertyChanged();
            RefreshCurrentSetFocus(value);
            RefreshBottomState();
        }
    }

    public void Initialize(IDispatcher dispatcher)
    {
        session = activeWorkoutService.GetActiveWorkout();

        if (session is null && workoutSelectionService.SelectedWorkoutId.HasValue)
            session = activeWorkoutService.StartWorkout(workoutSelectionService.SelectedWorkoutId.Value);

        ApplyKeepScreenAwake(true);
        SyncExercisesFromSession();

        if (elapsedTimer is null)
        {
            elapsedTimer = dispatcher.CreateTimer();
            elapsedTimer.Interval = TimeSpan.FromSeconds(1);
            elapsedTimer.Tick += OnElapsedTimerTick;
        }

        if (restTimer is null)
        {
            restTimer = dispatcher.CreateTimer();
            restTimer.Interval = TimeSpan.FromSeconds(1);
            restTimer.Tick += OnRestTimerTick;
        }

        elapsedTimer.Start();

        if (session?.IsResting == true)
            restTimer.Start();

        if (CurrentExercise is null)
            SelectCurrentSessionExercise();

        RefreshElapsed();
        RefreshRestText();
        RefreshBottomState();
        RefreshCurrentSetFocus(IsResting);
    }

    public void StopTimers()
    {
        elapsedTimer?.Stop();
        restTimer?.Stop();

        ApplyKeepScreenAwake(false);
    }

    [RelayCommand]
    private async Task Back()
    {
        if (HasWorkoutProgress)
        {
            IsLeaveWorkoutDialogVisible = true;
            return;
        }

        StopTimers();

        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    private void CancelLeaveWorkout()
    {
        IsLeaveWorkoutDialogVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmLeaveWorkout()
    {
        IsLeaveWorkoutDialogVisible = false;

        StopTimers();

        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    private async Task AddExercise()
    {
        StopTimers();

        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    private void SelectExercise(ActiveWorkoutExercisePresentationModel? exercise)
    {
        if (session is null || exercise is null)
            return;

        var index = Exercises.IndexOf(exercise);

        if (index < 0)
            return;

        activeWorkoutService.MoveToExercise(session.Id, index);
        session.CurrentExerciseIndex = index;

        SetCurrentExercise(index);
    }

    [RelayCommand]
    private void PreviousExercise()
    {
        if (session is null || Exercises.Count == 0)
            return;

        var nextIndex = Math.Max(0, session.CurrentExerciseIndex - 1);

        activeWorkoutService.MoveToExercise(session.Id, nextIndex);
        session.CurrentExerciseIndex = nextIndex;

        SetCurrentExercise(nextIndex);
    }

    [RelayCommand]
    private void NextExercise()
    {
        if (session is null || Exercises.Count == 0)
            return;

        var nextIndex = Math.Min(Exercises.Count - 1, session.CurrentExerciseIndex + 1);

        activeWorkoutService.MoveToExercise(session.Id, nextIndex);
        session.CurrentExerciseIndex = nextIndex;

        SetCurrentExercise(nextIndex);
    }

    [RelayCommand]
    private void SkipCurrentSet()
    {
        if (IsResting)
        {
            SkipFocusedSetDuringRest();
            return;
        }

        var set = CurrentSet;

        if (set is null)
            return;

        activeWorkoutService.SkipSet(set.Id);

        set.IsSkipped = true;
        set.IsCompleted = false;

        MoveAfterSetFinished(set);
    }

    [RelayCommand]
    private async Task PrimaryAction()
    {
        if (CanCompleteWorkout)
        {
            await CompleteWorkoutAsync();
            return;
        }

        if (IsResting)
        {
            SkipRest();
            return;
        }

        CompleteCurrentSet();
    }

    [RelayCommand]
    private void ShowProgress()
    {
        if (CurrentExercise is null)
            return;

        ExerciseHistoryDialogTitle = CurrentExercise.Name;
        ExerciseHistoryDialogMessage = "Previous logged sets for this exercise.";

        LoadExerciseHistory(
            CurrentExercise.WorkoutExerciseId,
            CurrentExercise.CatalogExerciseId,
            CurrentExercise.Name);

        IsExerciseHistoryDialogVisible = true;
    }

    [RelayCommand]
    private void CloseExerciseHistory()
    {
        IsExerciseHistoryDialogVisible = false;
    }

    [RelayCommand]
    private void CloseInfoDialog()
    {
        IsInfoDialogVisible = false;
    }

    [RelayCommand]
    private void CancelCompletePartialWorkout()
    {
        IsCompletePartialWorkoutDialogVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmCompletePartialWorkout()
    {
        IsCompletePartialWorkoutDialogVisible = false;

        await CompleteWorkoutCoreAsync();
    }

    [RelayCommand]
    private async Task CloseWorkoutComplete()
    {
        IsWorkoutCompleteDialogVisible = false;

        await Shell.Current.GoToAsync("..", true);
    }

    private void SyncExercisesFromSession()
    {
        Exercises.Clear();

        if (session is null)
        {
            CurrentExercise = null;
            ClearSetFocus();
            RefreshExerciseState();
            RefreshBottomState();
            return;
        }

        foreach (var exercise in activeWorkoutService.GetExercises(session.Id))
            Exercises.Add(ToPresentationModel(exercise));

        RefreshExerciseState();

        if (Exercises.Count == 0)
        {
            CurrentExercise = null;
            ClearSetFocus();
            RefreshBottomState();
            return;
        }

        if (session.CurrentExerciseIndex < 0 || session.CurrentExerciseIndex >= Exercises.Count)
        {
            session.CurrentExerciseIndex = 0;
            activeWorkoutService.MoveToExercise(session.Id, 0);
        }

        SetCurrentExercise(session.CurrentExerciseIndex);
    }

    private void CompleteCurrentSet()
    {
        var set = CurrentSet;

        if (set is null)
            return;

        activeWorkoutService.CompleteSet(
            set.Id,
            set.RepsValue,
            set.WeightKgValue,
            set.RestSeconds);

        set.IsCompleted = true;
        set.IsSkipped = false;

        MoveAfterSetFinished(set);
    }

    private void MoveAfterSetFinished(ActiveWorkoutSetPresentationModel set)
    {
        OnPropertyChanged(nameof(CurrentSet));
        OnPropertyChanged(nameof(HasCurrentSet));
        OnPropertyChanged(nameof(HasWorkoutProgress));
        OnPropertyChanged(nameof(HasPartialProgress));

        if (session is null || CurrentExercise is null)
            return;

        if (CurrentExercise.Sets.Any(item => !item.IsCompleted && !item.IsSkipped))
        {
            StartRest(set.RestSeconds);
            return;
        }

        var nextExercise = Exercises.FirstOrDefault(exercise =>
            exercise.Sets.Any(item => !item.IsCompleted && !item.IsSkipped));

        if (nextExercise is not null)
        {
            SelectExercise(nextExercise);
            StartRest(set.RestSeconds);
            return;
        }

        CanCompleteWorkout = Exercises.Count > 0;
        StopRestInternal();
        ClearSetFocus();
        RestTimerText = "All sets done";
        RefreshBottomState();
    }

    private void StartRest(int seconds)
    {
        if (session is null)
            return;

        if (seconds <= 0)
        {
            SkipRest();
            return;
        }

        activeWorkoutService.StartRest(session.Id, seconds);

        session.RemainingRestSeconds = seconds;
        IsResting = true;

        RefreshCurrentSetFocus(isWaitingForRest: true);
        RefreshRestText();

        restTimer?.Start();
        RefreshBottomState();
    }

    private void TickRestTimer()
    {
        if (session is null || !IsResting)
            return;

        activeWorkoutService.TickRest(session.Id);

        var refreshed = activeWorkoutService.GetSession(session.Id);

        if (refreshed is null)
            return;

        session = refreshed;

        if (session.RemainingRestSeconds <= 0)
        {
            NotifyRestFinished();
            SkipRest();
            return;
        }

        RefreshRestText();
    }

    private void SkipRest()
    {
        if (session is null)
            return;

        activeWorkoutService.StopRest(session.Id);
        StopRestInternal();

        RestTimerText = "Ready";

        SelectFirstPendingExercise();
        RefreshCurrentSetFocus(isWaitingForRest: false);
        RefreshBottomState();
    }

    private void SkipFocusedSetDuringRest()
    {
        if (session is null)
            return;

        var set = CurrentSet;

        if (set is null)
            return;

        activeWorkoutService.SkipSet(set.Id);

        set.IsSkipped = true;
        set.IsCompleted = false;

        OnPropertyChanged(nameof(CurrentSet));
        OnPropertyChanged(nameof(HasCurrentSet));
        OnPropertyChanged(nameof(HasWorkoutProgress));
        OnPropertyChanged(nameof(HasPartialProgress));

        var nextExercise = Exercises.FirstOrDefault(exercise =>
            exercise.Sets.Any(item => !item.IsCompleted && !item.IsSkipped));

        if (nextExercise is null)
        {
            CanCompleteWorkout = Exercises.Count > 0;
            StopRestInternal();
            ClearSetFocus();
            RestTimerText = "All sets done";
            RefreshBottomState();
            return;
        }

        SelectExercise(nextExercise);

        if (IsResting)
            RefreshCurrentSetFocus(isWaitingForRest: true);

        RefreshBottomState();
    }

    private void StopRestInternal()
    {
        restTimer?.Stop();

        if (session is not null)
        {
            session.RemainingRestSeconds = 0;
            session.IsResting = false;
            session.RestStartedAt = null;
            session.RestDurationSeconds = 0;
        }

        OnPropertyChanged(nameof(IsResting));
        RefreshCurrentSetFocus(isWaitingForRest: false);
    }

    private void SelectFirstPendingExercise()
    {
        if (session is null)
            return;

        var pending = Exercises.FirstOrDefault(exercise =>
            exercise.Sets.Any(set => !set.IsCompleted && !set.IsSkipped));

        if (pending is null)
        {
            CanCompleteWorkout = Exercises.Count > 0;
            ClearSetFocus();
            RefreshBottomState();
            return;
        }

        SelectExercise(pending);
    }

    private void SelectCurrentSessionExercise()
    {
        if (session is null || Exercises.Count == 0)
            return;

        var index = Math.Clamp(session.CurrentExerciseIndex, 0, Exercises.Count - 1);
        session.CurrentExerciseIndex = index;

        SetCurrentExercise(index);
    }

    private async Task CompleteWorkoutAsync()
    {
        if (session is null)
            return;

        if (HasPartialProgress)
        {
            IsCompletePartialWorkoutDialogVisible = true;
            return;
        }

        await CompleteWorkoutCoreAsync();
    }

    private async Task CompleteWorkoutCoreAsync()
    {
        if (session is null)
            return;

        if (!Exercises.Any(exercise => exercise.Sets.Any(set => set.IsCompleted)))
        {
            ShowInfoDialog(
                "No completed sets",
                "Complete at least one set before saving this workout to history.");
            return;
        }

        StopTimers();

        try
        {
            activeWorkoutService.CompleteWorkout(session.Id, HasPartialProgress);
        }
        catch (ServiceValidationException ex)
        {
            ShowInfoDialog("Could not complete workout", ex.Message);
            return;
        }

        session = null;
        ClearSetFocus();
        Exercises.Clear();
        RefreshExerciseState();

        CurrentExercise = null;
        CanCompleteWorkout = false;
        RestTimerText = "Ready";
        PrimaryButtonText = "Done";

        IsWorkoutCompleteDialogVisible = true;

        await Task.CompletedTask;
    }

    private void SetCurrentExercise(int index)
    {
        if (session is null)
            return;

        if (index < 0 || index >= Exercises.Count)
            return;

        session.CurrentExerciseIndex = index;

        for (var i = 0; i < Exercises.Count; i++)
            Exercises[i].IsSelected = i == index;

        CurrentExercise = Exercises[index];

        RefreshExerciseState();
        RefreshCurrentSetFocus(IsResting);
        RefreshBottomState();
    }

    private void RefreshElapsed()
    {
        if (session is null)
        {
            ElapsedText = "00:00";
            return;
        }

        var elapsed = DateTime.Now - session.StartedAt;

        ElapsedText = elapsed.TotalHours >= 1
            ? $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
            : $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private void RefreshRestText()
    {
        if (session is null)
        {
            RestTimerText = "Ready";
            return;
        }

        if (!session.IsResting)
        {
            RestTimerText = CanCompleteWorkout ? "All sets done" : "Ready";
            return;
        }

        var remaining = Math.Max(0, session.RemainingRestSeconds);
        var minutes = remaining / 60;
        var seconds = remaining % 60;

        RestTimerText = $"{minutes:00}:{seconds:00}";
    }

    private void RefreshBottomState()
    {
        if (CanCompleteWorkout)
        {
            PrimaryButtonText = "Complete workout";
            return;
        }

        PrimaryButtonText = IsResting ? "Skip rest" : "Done";
    }

    private void RefreshExerciseState()
    {
        OnPropertyChanged(nameof(HasExercises));
        OnPropertyChanged(nameof(IsWorkoutEmpty));
        OnPropertyChanged(nameof(CurrentSet));
        OnPropertyChanged(nameof(HasCurrentExercise));
        OnPropertyChanged(nameof(HasNoCurrentExercise));
        OnPropertyChanged(nameof(HasCurrentSet));
        OnPropertyChanged(nameof(HasWorkoutProgress));
        OnPropertyChanged(nameof(HasPartialProgress));
    }

    private void RefreshCurrentSetFocus(bool isWaitingForRest)
    {
        var currentSet = CurrentSet;

        foreach (var set in Exercises.SelectMany(exercise => exercise.Sets))
        {
            var isCurrent = currentSet is not null && set.Id == currentSet.Id;

            set.IsCurrentSet = isCurrent;
            set.IsWaitingForRest = isCurrent && isWaitingForRest;
        }
    }

    private void ClearSetFocus()
    {
        foreach (var set in Exercises.SelectMany(exercise => exercise.Sets))
        {
            set.IsCurrentSet = false;
            set.IsWaitingForRest = false;
        }
    }

    private async void NotifyRestFinished()
    {
        if (!userSettingsService.RestTimerSoundEnabled())
            return;

        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
        }

        try
        {
            restFinishedPlayer?.Dispose();

            var stream = await FileSystem.OpenAppPackageFileAsync("rest_finished.mp3");
            restFinishedPlayer = audioManager.CreatePlayer(stream);
            restFinishedPlayer.Play();
        }
        catch
        {
        }
    }

    private void ApplyKeepScreenAwake(bool isWorkoutScreenActive)
    {
        try
        {
            DeviceDisplay.Current.KeepScreenOn =
                isWorkoutScreenActive &&
                userSettingsService.KeepScreenAwakeDuringWorkout();
        }
        catch
        {
        }
    }

    private void ShowInfoDialog(string title, string message)
    {
        InfoDialogTitle = title;
        InfoDialogMessage = message;
        IsInfoDialogVisible = true;
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

    private ActiveWorkoutExercisePresentationModel ToPresentationModel(ActiveWorkoutExerciseModel model)
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

        var presentation = new ActiveWorkoutExercisePresentationModel
        {
            Id = model.Id,
            ActiveWorkoutSessionId = model.ActiveWorkoutSessionId,
            WorkoutExerciseId = model.WorkoutExerciseId,
            CatalogExerciseId = model.CatalogExerciseId,
            SortNumber = model.SortNumber,
            Name = model.Name,
            ImageSource = string.IsNullOrWhiteSpace(model.ImageSource) ? "image.png" : model.ImageSource,
            Notes = model.Notes,
            Metadata = metadata
        };

        foreach (var set in activeWorkoutService.GetSets(model.Id))
        {
            presentation.Sets.Add(new ActiveWorkoutSetPresentationModel
            {
                Id = set.Id,
                ActiveWorkoutExerciseId = set.ActiveWorkoutExerciseId,
                SortNumber = set.SortNumber,
                Reps = set.Reps.ToString(CultureInfo.InvariantCulture),
                WeightKg = set.WeightKg?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty,
                RestSeconds = set.RestSeconds,
                IsCompleted = set.IsCompleted,
                IsSkipped = set.IsSkipped
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

    private void OnElapsedTimerTick(object? sender, EventArgs e)
    {
        RefreshElapsed();
    }

    private void OnRestTimerTick(object? sender, EventArgs e)
    {
        TickRestTimer();
    }
}