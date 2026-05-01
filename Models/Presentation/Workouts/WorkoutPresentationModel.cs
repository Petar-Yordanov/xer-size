using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Common;
using XerSize.Models.Presentation.Options;

namespace XerSize.Models.Presentation.Workouts;

public sealed partial class WorkoutPresentationModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial Guid Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortNumberText))]
    public partial int SortNumber { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrainingTypeText))]
    [NotifyPropertyChangedFor(nameof(SettingsText))]
    public partial TrainingType TrainingType { get; set; } = TrainingType.Strength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SettingsText))]
    public partial bool ExcludeVolumeFromMetrics { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SettingsText))]
    public partial bool ExcludeCaloriesFromMetrics { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SettingsText))]
    public partial bool ExcludeMetadataFromMetrics { get; set; }

    public string TrainingTypeText => ExercisePresentationOptions.ToDisplayName(TrainingType);

    public string SortNumberText => PresentationFormatting.FormatPosition(SortNumber);

    public string SettingsText
    {
        get
        {
            var excluded = new List<string>();

            if (ExcludeVolumeFromMetrics)
                excluded.Add("volume excluded");

            if (ExcludeCaloriesFromMetrics)
                excluded.Add("calories excluded");

            if (ExcludeMetadataFromMetrics)
                excluded.Add("metadata excluded");

            if (excluded.Count == 0)
                return $"{TrainingTypeText} • included in metrics";

            return $"{TrainingTypeText} • {string.Join(" • ", excluded)}";
        }
    }
}