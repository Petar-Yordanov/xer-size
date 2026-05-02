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
    private DateTime completedAt;

    [ObservableProperty]
    private int durationMinutes;

    [ObservableProperty]
    private bool isPartial;

    [ObservableProperty]
    private int plannedSetCount;

    [ObservableProperty]
    private double estimatedCaloriesBurned;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpandIconSource))]
    private bool isExpanded;

    public ObservableCollection<HistoryExercisePresentationModel> Exercises { get; } = new();

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public int CompletedSetCount => Exercises.Sum(exercise => exercise.CompletedSetCount);

    public int SkippedSetCount => Exercises.Sum(exercise => exercise.SkippedSetCount);

    public double TotalVolumeKg => Exercises.Sum(exercise => exercise.TotalVolumeKg);

    public string CompletionStatus => IsPartial
        ? "Partial"
        : "Complete";

    public string CompletedAtText => CompletedAt.ToString("dd MMM yyyy");

    public string DurationText => FormatMinutes(DurationMinutes);

    public string CaloriesText => EstimatedCaloriesBurned <= 0
        ? "0 kcal"
        : $"{EstimatedCaloriesBurned:0} kcal";

    public string VolumeText => PresentationFormatting.FormatVolumeKg(TotalVolumeKg);

    public string SummaryText
    {
        get
        {
            var setText = PlannedSetCount == 1
                ? "1 planned set"
                : $"{PlannedSetCount} planned sets";

            var doneText = CompletedSetCount == 1
                ? "1 done"
                : $"{CompletedSetCount} done";

            if (SkippedSetCount > 0)
                return $"{CompletedAtText} • {DurationText} • {doneText} • {SkippedSetCount} skipped • {setText}";

            return $"{CompletedAtText} • {DurationText} • {doneText} • {setText}";
        }
    }

    public string ExpandIconSource => IsExpanded
        ? "expand_less.png"
        : "expand_more.png";

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(CompletedSetCount));
        OnPropertyChanged(nameof(SkippedSetCount));
        OnPropertyChanged(nameof(TotalVolumeKg));
        OnPropertyChanged(nameof(CompletionStatus));
        OnPropertyChanged(nameof(CompletedAtText));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(CaloriesText));
        OnPropertyChanged(nameof(VolumeText));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnNotesChanged(string value)
    {
        OnPropertyChanged(nameof(HasNotes));
    }

    partial void OnCompletedAtChanged(DateTime value)
    {
        NotifyCalculatedPropertiesChanged();
    }

    partial void OnDurationMinutesChanged(int value)
    {
        NotifyCalculatedPropertiesChanged();
    }

    partial void OnEstimatedCaloriesBurnedChanged(double value)
    {
        NotifyCalculatedPropertiesChanged();
    }

    partial void OnIsPartialChanged(bool value)
    {
        NotifyCalculatedPropertiesChanged();
    }

    partial void OnPlannedSetCountChanged(int value)
    {
        NotifyCalculatedPropertiesChanged();
    }

    private void OnExercisesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyCalculatedPropertiesChanged();
    }

    private static string FormatMinutes(int minutes)
    {
        minutes = Math.Max(0, minutes);

        if (minutes < 60)
            return $"{minutes} min";

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return remainingMinutes == 0
            ? $"{hours}h"
            : $"{hours}h {remainingMinutes}m";
    }
}