using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;

namespace XerSize.Models.Presentation.ExerciseMetadata;

public sealed class ExerciseMetadataPresentationModel : ObservableObject
{
    private ExerciseForce? force;
    private ExerciseBodyCategory? bodyCategory;
    private ExerciseMechanic? mechanic;
    private ExerciseEquipment? equipment;
    private LimbInvolvement? limbInvolvement;
    private MovementPattern? movementPattern;

    public ExerciseMetadataPresentationModel()
    {
        PrimaryMuscleCategories.CollectionChanged += OnMetadataCollectionChanged;
        SecondaryMuscleCategories.CollectionChanged += OnMetadataCollectionChanged;
        PrimaryMuscles.CollectionChanged += OnMetadataCollectionChanged;
        SecondaryMuscles.CollectionChanged += OnMetadataCollectionChanged;
    }

    public ExerciseForce? Force
    {
        get => force;
        set
        {
            if (SetProperty(ref force, value))
            {
                OnPropertyChanged(nameof(ForceText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public ExerciseBodyCategory? BodyCategory
    {
        get => bodyCategory;
        set
        {
            if (SetProperty(ref bodyCategory, value))
            {
                OnPropertyChanged(nameof(BodyCategoryText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public ExerciseMechanic? Mechanic
    {
        get => mechanic;
        set
        {
            if (SetProperty(ref mechanic, value))
            {
                OnPropertyChanged(nameof(MechanicText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public ExerciseEquipment? Equipment
    {
        get => equipment;
        set
        {
            if (SetProperty(ref equipment, value))
            {
                OnPropertyChanged(nameof(EquipmentText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public LimbInvolvement? LimbInvolvement
    {
        get => limbInvolvement;
        set
        {
            if (SetProperty(ref limbInvolvement, value))
            {
                OnPropertyChanged(nameof(LimbInvolvementText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public MovementPattern? MovementPattern
    {
        get => movementPattern;
        set
        {
            if (SetProperty(ref movementPattern, value))
            {
                OnPropertyChanged(nameof(MovementPatternText));
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    public ObservableCollection<string> PrimaryMuscleCategories { get; } = new();

    public ObservableCollection<string> SecondaryMuscleCategories { get; } = new();

    public ObservableCollection<string> PrimaryMuscles { get; } = new();

    public ObservableCollection<string> SecondaryMuscles { get; } = new();

    public string ForceText => ExercisePresentationOptions.ToDisplayName(Force);

    public string BodyCategoryText => ExercisePresentationOptions.ToDisplayName(BodyCategory);

    public string MechanicText => ExercisePresentationOptions.ToDisplayName(Mechanic);

    public string EquipmentText => ExercisePresentationOptions.ToDisplayName(Equipment);

    public string LimbInvolvementText => ExercisePresentationOptions.ToDisplayName(LimbInvolvement);

    public string MovementPatternText => ExercisePresentationOptions.ToDisplayName(MovementPattern);

    public bool HasPrimaryMuscleCategories => PrimaryMuscleCategories.Count > 0;

    public bool HasSecondaryMuscleCategories => SecondaryMuscleCategories.Count > 0;

    public bool HasPrimaryMuscles => PrimaryMuscles.Count > 0;

    public bool HasSecondaryMuscles => SecondaryMuscles.Count > 0;

    public string SearchText => string.Join(
        " ",
        new[]
        {
            ForceText,
            BodyCategoryText,
            MechanicText,
            EquipmentText,
            LimbInvolvementText,
            MovementPatternText,
            string.Join(" ", PrimaryMuscleCategories),
            string.Join(" ", SecondaryMuscleCategories),
            string.Join(" ", PrimaryMuscles),
            string.Join(" ", SecondaryMuscles)
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

    public void NotifyCollectionDerivedPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasPrimaryMuscleCategories));
        OnPropertyChanged(nameof(HasSecondaryMuscleCategories));
        OnPropertyChanged(nameof(HasPrimaryMuscles));
        OnPropertyChanged(nameof(HasSecondaryMuscles));
        OnPropertyChanged(nameof(SearchText));
    }

    private void OnMetadataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionDerivedPropertiesChanged();
    }
}