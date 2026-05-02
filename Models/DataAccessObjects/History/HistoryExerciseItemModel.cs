using XerSize.Models.Definitions;

namespace XerSize.Models.DataAccessObjects.History;

public sealed class HistoryExerciseItemModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HistoryWorkoutId { get; set; }

    public string CatalogExerciseId { get; set; } = string.Empty;

    public Guid? WorkoutExerciseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ImageSource { get; set; } = string.Empty;

    public ExerciseTrackingMode TrackingMode { get; set; } = ExerciseTrackingMode.Strength;

    public ExerciseForce? Force { get; set; }

    public ExerciseBodyCategory? BodyCategory { get; set; }

    public ExerciseMechanic? Mechanic { get; set; }

    public ExerciseEquipment? Equipment { get; set; }

    public LimbInvolvement? LimbInvolvement { get; set; }

    public MovementPattern? MovementPattern { get; set; }

    public List<string> PrimaryMuscleCategories { get; set; } = [];

    public List<string> SecondaryMuscleCategories { get; set; } = [];

    public List<string> PrimaryMuscles { get; set; } = [];

    public List<string> SecondaryMuscles { get; set; } = [];

    public DateTime CompletedAt { get; set; }
}