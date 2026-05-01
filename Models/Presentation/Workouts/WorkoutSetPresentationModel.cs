using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.Workouts;

public partial class WorkoutSetPresentationModel : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid workoutExerciseId;

    [ObservableProperty]
    private int sortNumber;

    [ObservableProperty]
    private int reps;

    [ObservableProperty]
    private double? weightKg;

    [ObservableProperty]
    private int restSeconds;

    public string SortNumberText => PresentationFormatting.FormatSetLabel(SortNumber);

    public string RepsText => PresentationFormatting.FormatReps(Reps);

    public string WeightText => PresentationFormatting.FormatWeightKg(WeightKg);

    public string RestText => PresentationFormatting.FormatRestSeconds(RestSeconds);

    partial void OnSortNumberChanged(int value)
    {
        OnPropertyChanged(nameof(SortNumberText));
    }

    partial void OnRepsChanged(int value)
    {
        OnPropertyChanged(nameof(RepsText));
    }

    partial void OnWeightKgChanged(double? value)
    {
        OnPropertyChanged(nameof(WeightText));
    }

    partial void OnRestSecondsChanged(int value)
    {
        OnPropertyChanged(nameof(RestText));
    }
}