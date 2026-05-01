using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.History;

public sealed partial class HistoryWorkoutPresentationModel : ObservableObject
{
    public HistoryWorkoutPresentationModel()
    {
        Exercises.CollectionChanged += OnExercisesChanged;
    }

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid workoutId;

    [ObservableProperty]
    private string workoutName = string.Empty;

    [ObservableProperty]
    private DateTime startedAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompletedAtText))]
    [NotifyPropertyChangedFor(nameof(CompletedTimeText))]
    private DateTime completedAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(SummaryText))]
    private int durationMinutes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompletionStatus))]
    [NotifyPropertyChangedFor(nameof(SetsText))]
    private bool isPartial;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetsText))]
    [NotifyPropertyChangedFor(nameof(SummaryText))]
    private int plannedSetCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumeTrendIconSource))]
    [NotifyPropertyChangedFor(nameof(VolumeTrendColor))]
    [NotifyPropertyChangedFor(nameof(HasVolumeTrendIcon))]
    private double volumeTrendDeltaKg;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotes))]
    private string notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpandIconSource))]
    private bool isExpanded;

    public ObservableCollection<HistoryExercisePresentationModel> Exercises { get; } = new();

    public int ExerciseCount => Exercises.Count;

    public int CompletedSetCount => Exercises.Sum(exercise => exercise.CompletedSetCount);

    public int SkippedSetCount => Exercises.Sum(exercise => exercise.SkippedSetCount);

    public double TotalVolumeKg => Exercises.Sum(exercise => exercise.TotalVolumeKg);

    public string VolumeTrendIconSource
    {
        get
        {
            if (VolumeTrendDeltaKg > 0)
                return "expand_less.png";

            if (VolumeTrendDeltaKg < 0)
                return "expand_more.png";

            return string.Empty;
        }
    }

    public Color VolumeTrendColor
    {
        get
        {
            if (VolumeTrendDeltaKg > 0)
                return Colors.Green;

            if (VolumeTrendDeltaKg < 0)
                return Colors.Red;

            return Colors.Transparent;
        }
    }

    public bool HasVolumeTrendIcon => !string.IsNullOrWhiteSpace(VolumeTrendIconSource);

    public bool HasExercises => Exercises.Count > 0;

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public string CompletionStatus => IsPartial ? "Partial" : "Completed";

    public string CompletedAtText => CompletedAt.ToString("dd MMM yyyy");

    public string CompletedTimeText => CompletedAt.ToString("HH:mm");

    public string DurationText => $"{DurationMinutes} min";

    public string ExercisesText => ExerciseCount == 1
        ? "1 exercise"
        : $"{ExerciseCount} exercises";

    public string SetsText
    {
        get
        {
            if (IsPartial && PlannedSetCount > CompletedSetCount)
                return $"{CompletedSetCount}/{PlannedSetCount} sets";

            return CompletedSetCount == 1
                ? "1 set"
                : $"{CompletedSetCount} sets";
        }
    }

    public string VolumeText => PresentationFormatting.FormatVolumeKg(TotalVolumeKg);

    public string SummaryText => $"{ExercisesText} • {SetsText} • {DurationText}";

    public string ExpandIconSource => IsExpanded
        ? "expand_less.png"
        : "expand_more.png";

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(ExerciseCount));
        OnPropertyChanged(nameof(CompletedSetCount));
        OnPropertyChanged(nameof(SkippedSetCount));
        OnPropertyChanged(nameof(TotalVolumeKg));
        OnPropertyChanged(nameof(VolumeTrendIconSource));
        OnPropertyChanged(nameof(VolumeTrendColor));
        OnPropertyChanged(nameof(HasVolumeTrendIcon));
        OnPropertyChanged(nameof(HasExercises));
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(CompletionStatus));
        OnPropertyChanged(nameof(CompletedAtText));
        OnPropertyChanged(nameof(CompletedTimeText));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(ExercisesText));
        OnPropertyChanged(nameof(SetsText));
        OnPropertyChanged(nameof(VolumeText));
        OnPropertyChanged(nameof(SummaryText));
    }

    private void OnExercisesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<HistoryExercisePresentationModel>())
                item.PropertyChanged -= OnExercisePropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<HistoryExercisePresentationModel>())
                item.PropertyChanged += OnExercisePropertyChanged;
        }

        NotifyCalculatedPropertiesChanged();
    }

    private void OnExercisePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(HistoryExercisePresentationModel.CompletedSetCount)
            or nameof(HistoryExercisePresentationModel.SkippedSetCount)
            or nameof(HistoryExercisePresentationModel.TotalVolumeKg)
            or nameof(HistoryExercisePresentationModel.SummaryText)
            or nameof(HistoryExercisePresentationModel.SetsText))
        {
            NotifyCalculatedPropertiesChanged();
        }
    }
}