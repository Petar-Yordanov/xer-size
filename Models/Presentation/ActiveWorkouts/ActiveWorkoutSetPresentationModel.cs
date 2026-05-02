using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.ActiveWorkouts;

public sealed partial class ActiveWorkoutSetPresentationModel : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid activeWorkoutExerciseId;

    [ObservableProperty]
    private ExerciseTrackingMode trackingMode = ExerciseTrackingMode.Strength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortNumberText))]
    private int sortNumber;

    [ObservableProperty]
    private int restSeconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepsValue))]
    [NotifyPropertyChangedFor(nameof(VolumeKg))]
    [NotifyPropertyChangedFor(nameof(PrimaryMetricText))]
    private string reps = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeightKgValue))]
    [NotifyPropertyChangedFor(nameof(VolumeKg))]
    [NotifyPropertyChangedFor(nameof(SecondaryMetricText))]
    private string weightKg = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationSecondsValue))]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(PrimaryMetricText))]
    private string durationSeconds = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DistanceKmValue))]
    [NotifyPropertyChangedFor(nameof(DistanceMetersValue))]
    [NotifyPropertyChangedFor(nameof(DistanceText))]
    [NotifyPropertyChangedFor(nameof(SecondaryMetricText))]
    private string distanceKm = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(CurrentStatusText))]
    private bool isCompleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(CurrentStatusText))]
    private bool isSkipped;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCurrentSetIndicator))]
    [NotifyPropertyChangedFor(nameof(CurrentSetIndicatorText))]
    [NotifyPropertyChangedFor(nameof(CurrentStatusText))]
    private bool isCurrentSet;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCurrentSetIndicator))]
    [NotifyPropertyChangedFor(nameof(CurrentSetIndicatorText))]
    [NotifyPropertyChangedFor(nameof(CurrentStatusText))]
    private bool isWaitingForRest;

    public string SortNumberText => PresentationFormatting.FormatSetLabel(SortNumber);

    public bool IsStrength => TrackingMode == ExerciseTrackingMode.Strength;

    public bool IsTime => TrackingMode == ExerciseTrackingMode.Time;

    public bool IsTimeAndDistance => TrackingMode == ExerciseTrackingMode.TimeAndDistance;

    public bool IsDone
    {
        get => IsCompleted;
        set => IsCompleted = value;
    }

    public int RepsValue => PresentationFormatting.ParseNonNegativeInt(Reps);

    public double? WeightKgValue => PresentationFormatting.ParseNonNegativeNullableDouble(WeightKg);

    public int DurationSecondsValue => PresentationFormatting.ParseNonNegativeInt(DurationSeconds);

    public double? DistanceKmValue => PresentationFormatting.ParseNonNegativeNullableDouble(DistanceKm);

    public double? DistanceMetersValue => DistanceKmValue.HasValue
        ? Math.Max(0, DistanceKmValue.Value) * 1000d
        : null;

    public double VolumeKg => TrackingMode == ExerciseTrackingMode.Strength
        ? PresentationFormatting.CalculateVolumeKg(RepsValue, WeightKgValue)
        : 0;

    public string DurationText => PresentationFormatting.FormatDurationSeconds(DurationSecondsValue);

    public string DistanceText => PresentationFormatting.FormatDistanceMeters(DistanceMetersValue);

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
        _ => PresentationFormatting.FormatReps(RepsValue)
    };

    public string SecondaryMetricLabel => TrackingMode switch
    {
        ExerciseTrackingMode.Time => "Rest",
        ExerciseTrackingMode.TimeAndDistance => "Distance",
        _ => "Weight"
    };

    public string SecondaryMetricText => TrackingMode switch
    {
        ExerciseTrackingMode.Time => PresentationFormatting.FormatRestSeconds(RestSeconds),
        ExerciseTrackingMode.TimeAndDistance => DistanceText,
        _ => PresentationFormatting.FormatWeightKg(WeightKgValue)
    };

    public string TertiaryMetricLabel => "Rest";

    public string TertiaryMetricText => PresentationFormatting.FormatRestSeconds(RestSeconds);

    public bool ShowCurrentSetIndicator => IsCurrentSet || IsWaitingForRest;

    public string CurrentSetIndicatorText
    {
        get
        {
            if (IsWaitingForRest)
                return "Rest next";

            if (IsCurrentSet)
                return "Current";

            return string.Empty;
        }
    }

    public string CurrentStatusText
    {
        get
        {
            if (IsWaitingForRest)
                return "Rest next";

            if (IsCurrentSet)
                return "Current";

            return StatusText;
        }
    }

    public string StatusText
    {
        get
        {
            if (IsCompleted)
                return "Done";

            if (IsSkipped)
                return "Skipped";

            return "Todo";
        }
    }

    partial void OnTrackingModeChanged(ExerciseTrackingMode value)
    {
        OnPropertyChanged(nameof(IsStrength));
        OnPropertyChanged(nameof(IsTime));
        OnPropertyChanged(nameof(IsTimeAndDistance));
        OnPropertyChanged(nameof(VolumeKg));
        OnPropertyChanged(nameof(PrimaryMetricLabel));
        OnPropertyChanged(nameof(PrimaryMetricText));
        OnPropertyChanged(nameof(SecondaryMetricLabel));
        OnPropertyChanged(nameof(SecondaryMetricText));
        OnPropertyChanged(nameof(TertiaryMetricLabel));
        OnPropertyChanged(nameof(TertiaryMetricText));
    }

    partial void OnRestSecondsChanged(int value)
    {
        OnPropertyChanged(nameof(SecondaryMetricText));
        OnPropertyChanged(nameof(TertiaryMetricText));
    }

    partial void OnIsCompletedChanged(bool value)
    {
        if (value && IsSkipped)
            IsSkipped = false;

        OnPropertyChanged(nameof(IsDone));
        OnPropertyChanged(nameof(IsSkipped));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CurrentStatusText));
    }

    partial void OnIsSkippedChanged(bool value)
    {
        if (value && IsCompleted)
            IsCompleted = false;

        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsDone));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CurrentStatusText));
    }
}