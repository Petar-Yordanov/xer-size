using XerSize.Models.Definitions;

namespace XerSize.Models.DataAccessObjects.Workouts;

public sealed class WorkoutModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public int SortNumber { get; set; }

    public TrainingType TrainingType { get; set; } = TrainingType.Strength;

    public bool ExcludeVolumeFromMetrics { get; set; }

    public bool ExcludeCaloriesFromMetrics { get; set; }

    public bool ExcludeMetadataFromMetrics { get; set; }
}