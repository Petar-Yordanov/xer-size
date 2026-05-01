using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using XerSize.Models.Presentation.ExerciseMetadata;

namespace XerSize.Models.Presentation.History;

public sealed partial class HistoryExercisePresentationModel : ObservableObject
{
    public HistoryExercisePresentationModel()
    {
        CompletedSets.CollectionChanged += OnCompletedSetsChanged;
    }

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid historyWorkoutId;

    [ObservableProperty]
    private string catalogExerciseId = string.Empty;

    [ObservableProperty]
    private Guid? workoutExerciseId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string imageSource = string.Empty;

    [ObservableProperty]
    private ExerciseMetadataPresentationModel metadata = new();

    [ObservableProperty]
    private DateTime completedAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpandIconSource))]
    private bool isExpanded;

    public ObservableCollection<HistorySetPresentationModel> CompletedSets { get; } = new();

    public int CompletedSetCount => CompletedSets.Count(set => set.IsCompleted);

    public int SkippedSetCount => CompletedSets.Count(set => set.IsSkipped);

    public double TotalVolumeKg => CompletedSets.Sum(set => set.VolumeKg);

    public string SummaryText
    {
        get
        {
            if (CompletedSets.Count == 0)
                return "No sets";

            var totalReps = CompletedSets
                .Where(set => set.IsCompleted)
                .Sum(set => Math.Max(0, set.Reps));

            return TotalVolumeKg > 0
                ? $"{CompletedSetCount} done • {SkippedSetCount} skipped • {totalReps} reps • {TotalVolumeKg:0.#} kg"
                : $"{CompletedSetCount} done • {SkippedSetCount} skipped • {totalReps} reps";
        }
    }

    public string SetsText => CompletedSets.Count == 1
        ? "1 set"
        : $"{CompletedSets.Count} sets";

    public string ExpandIconSource => IsExpanded
        ? "expand_less.png"
        : "expand_more.png";

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(CompletedSetCount));
        OnPropertyChanged(nameof(SkippedSetCount));
        OnPropertyChanged(nameof(TotalVolumeKg));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(SetsText));
    }

    private void OnCompletedSetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyCalculatedPropertiesChanged();
    }
}