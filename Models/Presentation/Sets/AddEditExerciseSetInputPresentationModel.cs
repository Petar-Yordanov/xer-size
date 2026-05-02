using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.Sets;

public partial class AddEditExerciseSetInputPresentationModel : ObservableObject
{
    private int sortNumber;
    private string reps = "10";
    private string weightKg = "0";
    private string durationSeconds = "60";
    private string distanceKm = string.Empty;
    private string restSeconds = "90";

    public int SortNumber
    {
        get => sortNumber;
        set
        {
            if (SetProperty(ref sortNumber, value))
                OnPropertyChanged(nameof(SortNumberText));
        }
    }

    public string Reps
    {
        get => reps;
        set
        {
            if (SetProperty(ref reps, value))
            {
                OnPropertyChanged(nameof(RepsValue));
                OnPropertyChanged(nameof(VolumeKg));
            }
        }
    }

    public string WeightKg
    {
        get => weightKg;
        set
        {
            if (SetProperty(ref weightKg, value))
            {
                OnPropertyChanged(nameof(WeightKgValue));
                OnPropertyChanged(nameof(VolumeKg));
            }
        }
    }

    public string DurationSeconds
    {
        get => durationSeconds;
        set
        {
            if (SetProperty(ref durationSeconds, value))
            {
                OnPropertyChanged(nameof(DurationSecondsValue));
                OnPropertyChanged(nameof(DurationText));
            }
        }
    }

    public string DistanceKm
    {
        get => distanceKm;
        set
        {
            if (SetProperty(ref distanceKm, value))
            {
                OnPropertyChanged(nameof(DistanceKmValue));
                OnPropertyChanged(nameof(DistanceMetersValue));
                OnPropertyChanged(nameof(DistanceText));
            }
        }
    }

    public string RestSeconds
    {
        get => restSeconds;
        set
        {
            if (SetProperty(ref restSeconds, value))
            {
                OnPropertyChanged(nameof(RestSecondsValue));
                OnPropertyChanged(nameof(RestText));
            }
        }
    }

    public string SortNumberText => PresentationFormatting.FormatSetLabel(SortNumber);

    public int RepsValue => PresentationFormatting.ParseNonNegativeInt(Reps);

    public double? WeightKgValue => PresentationFormatting.ParseNonNegativeNullableDouble(WeightKg);

    public int DurationSecondsValue => PresentationFormatting.ParseNonNegativeInt(DurationSeconds);

    public double? DistanceKmValue => PresentationFormatting.ParseNonNegativeNullableDouble(DistanceKm);

    public double? DistanceMetersValue => DistanceKmValue.HasValue
        ? DistanceKmValue.Value * 1000d
        : null;

    public int RestSecondsValue => PresentationFormatting.ParseNonNegativeInt(RestSeconds);

    public string DurationText => FormatDuration(DurationSecondsValue);

    public string DistanceText => DistanceKmValue.HasValue && DistanceKmValue.Value > 0
        ? $"{DistanceKmValue.Value:0.##} km"
        : "0 km";

    public string RestText => PresentationFormatting.FormatRestSeconds(RestSecondsValue);

    public double VolumeKg => PresentationFormatting.CalculateVolumeKg(RepsValue, WeightKgValue);

    private static string FormatDuration(int seconds)
    {
        seconds = Math.Max(0, seconds);

        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;

        return minutes > 0
            ? $"{minutes}:{remainingSeconds:00}"
            : $"{remainingSeconds}s";
    }
}