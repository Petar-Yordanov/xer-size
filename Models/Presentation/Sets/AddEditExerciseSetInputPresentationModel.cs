using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.Sets;

public partial class AddEditExerciseSetInputPresentationModel : ObservableObject
{
    private int sortNumber;
    private string reps = "10";
    private string weightKg = "0";
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

    public int RestSecondsValue => PresentationFormatting.ParseNonNegativeInt(RestSeconds);

    public string RestText => PresentationFormatting.FormatRestSeconds(RestSecondsValue);

    public double VolumeKg => PresentationFormatting.CalculateVolumeKg(RepsValue, WeightKgValue);
}