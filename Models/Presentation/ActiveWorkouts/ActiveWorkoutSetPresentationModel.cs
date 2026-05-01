using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.ActiveWorkouts;

public sealed partial class ActiveWorkoutSetPresentationModel : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid activeWorkoutExerciseId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortNumberText))]
    private int sortNumber;

    [ObservableProperty]
    private int restSeconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepsValue))]
    [NotifyPropertyChangedFor(nameof(VolumeKg))]
    private string reps = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeightKgValue))]
    [NotifyPropertyChangedFor(nameof(VolumeKg))]
    private string weightKg = string.Empty;

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

    public bool IsDone
    {
        get => IsCompleted;
        set => IsCompleted = value;
    }

    public int RepsValue => PresentationFormatting.ParseNonNegativeInt(Reps);

    public double? WeightKgValue => PresentationFormatting.ParseNonNegativeNullableDouble(WeightKg);

    public double VolumeKg => PresentationFormatting.CalculateVolumeKg(RepsValue, WeightKgValue);

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