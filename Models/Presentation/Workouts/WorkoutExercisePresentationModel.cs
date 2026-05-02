using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Common;
using XerSize.Models.Presentation.ExerciseMetadata;

namespace XerSize.Models.Presentation.Workouts;

public sealed partial class WorkoutExercisePresentationModel : ObservableObject
{
    public WorkoutExercisePresentationModel()
    {
        Sets.CollectionChanged += OnSetsChanged;
    }

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid workoutId;

    [ObservableProperty]
    private string catalogExerciseId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchText))]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchText))]
    private string notes = string.Empty;

    [ObservableProperty]
    private int sortNumber;

    [ObservableProperty]
    private string imageSource = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrackingModeText))]
    [NotifyPropertyChangedFor(nameof(SummaryText))]
    private ExerciseTrackingMode trackingMode = ExerciseTrackingMode.Strength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryText))]
    [NotifyPropertyChangedFor(nameof(SearchText))]
    private ExerciseMetadataPresentationModel metadata = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpandIconSource))]
    private bool isExpanded;

    public ObservableCollection<WorkoutSetPresentationModel> Sets { get; } = new();

    public string ExpandIconSource => IsExpanded
        ? "expand_less.png"
        : "expand_more.png";

    public string TrackingModeText => TrackingMode switch
    {
        ExerciseTrackingMode.Time => "Time",
        ExerciseTrackingMode.TimeAndDistance => "Time + Distance",
        _ => "Weight"
    };

    public string SummaryText
    {
        get
        {
            if (Sets.Count == 0)
                return $"No sets • {TrackingModeText}";

            var setText = Sets.Count == 1
                ? "1 set"
                : $"{Sets.Count} sets";

            var detailText = TrackingMode switch
            {
                ExerciseTrackingMode.Time => PresentationFormatting.FormatDurationSeconds(
                    Sets.Sum(set => Math.Max(0, set.DurationSeconds))),
                ExerciseTrackingMode.TimeAndDistance => PresentationFormatting.FormatDistanceMeters(
                    Sets.Sum(set => Math.Max(0, set.DistanceMeters ?? 0))),
                _ => string.Empty
            };

            var muscleText = Metadata.PrimaryMuscleCategories.Count == 0
                ? string.Empty
                : $" • {string.Join(", ", Metadata.PrimaryMuscleCategories.Take(2))}";

            return string.IsNullOrWhiteSpace(detailText)
                ? $"{setText} • {TrackingModeText}{muscleText}"
                : $"{setText} • {detailText} • {TrackingModeText}{muscleText}";
        }
    }

    public string SearchText => string.Join(
        " ",
        new[]
        {
            Name,
            Notes,
            TrackingModeText,
            Metadata.SearchText
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

    public void NotifyDisplayPropertiesChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(SortNumber));
        OnPropertyChanged(nameof(ImageSource));
        OnPropertyChanged(nameof(TrackingMode));
        OnPropertyChanged(nameof(TrackingModeText));
        OnPropertyChanged(nameof(Metadata));
        OnPropertyChanged(nameof(Sets));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(SearchText));
    }

    private void OnSetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Sets));
        OnPropertyChanged(nameof(SummaryText));
    }

    partial void OnMetadataChanged(ExerciseMetadataPresentationModel value)
    {
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(SearchText));
    }
}