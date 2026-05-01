using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Presentation.Common;
using XerSize.Models.Presentation.ExerciseMetadata;

namespace XerSize.Models.Presentation.ActiveWorkouts;

public sealed partial class ActiveWorkoutExercisePresentationModel : ObservableObject
{
    public ActiveWorkoutExercisePresentationModel()
    {
        Sets.CollectionChanged += OnSetsChanged;
    }

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private Guid activeWorkoutSessionId;

    [ObservableProperty]
    private Guid workoutExerciseId;

    [ObservableProperty]
    private string catalogExerciseId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortNumberText))]
    [NotifyPropertyChangedFor(nameof(BreadcrumbText))]
    private int sortNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotes))]
    [NotifyPropertyChangedFor(nameof(BreadcrumbText))]
    private string name = string.Empty;

    [ObservableProperty]
    private string imageSource = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotes))]
    private string notes = string.Empty;

    [ObservableProperty]
    private ExerciseMetadataPresentationModel metadata = new();

    [ObservableProperty]
    private bool isSelected;

    public ObservableCollection<ActiveWorkoutSetPresentationModel> Sets { get; } = new();

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public string SortNumberText => PresentationFormatting.FormatPosition(SortNumber);

    public string BreadcrumbText => string.IsNullOrWhiteSpace(Name)
        ? SortNumberText
        : $"{PresentationFormatting.FormatOrdinalPrefix(SortNumber)} {Name}";

    public int CompletedSetCount => Sets.Count(set => set.IsCompleted);

    public int TotalSetCount => Sets.Count;

    public bool HasCompletedSets => CompletedSetCount > 0;

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(SortNumberText));
        OnPropertyChanged(nameof(BreadcrumbText));
        OnPropertyChanged(nameof(CompletedSetCount));
        OnPropertyChanged(nameof(TotalSetCount));
        OnPropertyChanged(nameof(HasCompletedSets));
    }

    private void OnSetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<ActiveWorkoutSetPresentationModel>())
                item.PropertyChanged -= OnSetPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<ActiveWorkoutSetPresentationModel>())
                item.PropertyChanged += OnSetPropertyChanged;
        }

        NotifyCalculatedPropertiesChanged();
    }

    private void OnSetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ActiveWorkoutSetPresentationModel.IsCompleted)
            or nameof(ActiveWorkoutSetPresentationModel.IsSkipped))
        {
            NotifyCalculatedPropertiesChanged();
        }
    }
}