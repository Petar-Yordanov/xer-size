using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.Workouts;

public partial class WorkoutSetPresentationModel : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid workoutExerciseId;

    [ObservableProperty]
    private ExerciseTrackingMode trackingMode = ExerciseTrackingMode.Strength;

    [ObservableProperty]
    private int sortNumber;

    [ObservableProperty]
    private int reps;

    [ObservableProperty]
    private double? weightKg;

    [ObservableProperty]
    private int durationSeconds;

    [ObservableProperty]
    private double? distanceMeters;

    [ObservableProperty]
    private int restSeconds;

    public string SortNumberText => PresentationFormatting.FormatSetLabel(SortNumber);

    public bool IsStrength => TrackingMode == ExerciseTrackingMode.Strength;

    public bool IsTime => TrackingMode == ExerciseTrackingMode.Time;

    public bool IsTimeAndDistance => TrackingMode == ExerciseTrackingMode.TimeAndDistance;

    public string RepsText => PresentationFormatting.FormatReps(Reps);

    public string WeightText => PresentationFormatting.FormatWeightKg(WeightKg);

    public string DurationText => PresentationFormatting.FormatDurationSeconds(DurationSeconds);

    public string DistanceText => PresentationFormatting.FormatDistanceMeters(DistanceMeters);

    public string RestText => PresentationFormatting.FormatRestSeconds(RestSeconds);

    public string PrimaryMetricLabel => TrackingMode switch
    {
        ExerciseTrackingMode.Time => "Time",
        ExerciseTrackingMode.TimeAndDistance => "Time",
        _ => "Reps"
    };

    public string PrimaryMetricText => TrackingMode switch
    {
        ExerciseTrackingMode.Time => DurationText,
        ExerciseTrackingMode.TimeAndDistance => DurationText,
        _ => RepsText
    };

    public string SecondaryMetricLabel => TrackingMode switch
    {
        ExerciseTrackingMode.Time => "Rest",
        ExerciseTrackingMode.TimeAndDistance => "Distance",
        _ => "Weight"
    };

    public string SecondaryMetricText => TrackingMode switch
    {
        ExerciseTrackingMode.Time => RestText,
        ExerciseTrackingMode.TimeAndDistance => DistanceText,
        _ => WeightText
    };

    public string TertiaryMetricLabel => "Rest";

    public string TertiaryMetricText => RestText;

    partial void OnTrackingModeChanged(ExerciseTrackingMode value)
    {
        NotifyDisplayProperties();
    }

    partial void OnSortNumberChanged(int value)
    {
        OnPropertyChanged(nameof(SortNumberText));
    }

    partial void OnRepsChanged(int value)
    {
        NotifyDisplayProperties();
    }

    partial void OnWeightKgChanged(double? value)
    {
        NotifyDisplayProperties();
    }

    partial void OnDurationSecondsChanged(int value)
    {
        NotifyDisplayProperties();
    }

    partial void OnDistanceMetersChanged(double? value)
    {
        NotifyDisplayProperties();
    }

    partial void OnRestSecondsChanged(int value)
    {
        NotifyDisplayProperties();
    }

    private void NotifyDisplayProperties()
    {
        OnPropertyChanged(nameof(IsStrength));
        OnPropertyChanged(nameof(IsTime));
        OnPropertyChanged(nameof(IsTimeAndDistance));
        OnPropertyChanged(nameof(RepsText));
        OnPropertyChanged(nameof(WeightText));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(DistanceText));
        OnPropertyChanged(nameof(RestText));
        OnPropertyChanged(nameof(PrimaryMetricLabel));
        OnPropertyChanged(nameof(PrimaryMetricText));
        OnPropertyChanged(nameof(SecondaryMetricLabel));
        OnPropertyChanged(nameof(SecondaryMetricText));
        OnPropertyChanged(nameof(TertiaryMetricLabel));
        OnPropertyChanged(nameof(TertiaryMetricText));
    }
}