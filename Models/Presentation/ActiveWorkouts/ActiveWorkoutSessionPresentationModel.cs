using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace XerSize.Models.Presentation.ActiveWorkouts;

public sealed partial class ActiveWorkoutSessionPresentationModel : ObservableObject
{
    public ActiveWorkoutSessionPresentationModel()
    {
        Exercises.CollectionChanged += OnExercisesChanged;
    }

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid workoutId;

    [ObservableProperty]
    private string workoutName = "Workout";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ElapsedSeconds))]
    private DateTime startedAt = DateTime.Now;

    [ObservableProperty]
    private DateTime? completedAt;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private bool isPartial;

    [ObservableProperty]
    private int currentExerciseIndex;

    private int remainingRestSeconds;

    public int RemainingRestSeconds
    {
        get => remainingRestSeconds;
        set => SetProperty(ref remainingRestSeconds, Math.Max(0, value));
    }

    [ObservableProperty]
    private bool isResting;

    [ObservableProperty]
    private double? weightKgAtTime;

    [ObservableProperty]
    private int? ageAtTime;

    public ObservableCollection<ActiveWorkoutExercisePresentationModel> Exercises { get; } = new();

    public int ElapsedSeconds => Math.Max(0, (int)(DateTime.Now - StartedAt).TotalSeconds);

    public bool HasExercises => Exercises.Count > 0;

    public bool IsWorkoutEmpty => !HasExercises;

    public void NotifyTimerPropertiesChanged()
    {
        OnPropertyChanged(nameof(ElapsedSeconds));
        OnPropertyChanged(nameof(HasExercises));
        OnPropertyChanged(nameof(IsWorkoutEmpty));
    }

    private void OnExercisesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasExercises));
        OnPropertyChanged(nameof(IsWorkoutEmpty));
    }
}