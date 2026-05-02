namespace XerSize.Models.DataAccessObjects.History;

public sealed class HistoryWorkoutItemModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkoutId { get; set; }

    public string WorkoutName { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsPartial { get; set; }

    public int PlannedSetCount { get; set; }

    public int CompletedSetCount { get; set; }

    public int SkippedSetCount { get; set; }

    public double? WeightKgAtTime { get; set; }

    public int? AgeAtTime { get; set; }

    public bool ExcludeVolumeFromMetrics { get; set; }

    public bool ExcludeCaloriesFromMetrics { get; set; }

    public bool ExcludeMetadataFromMetrics { get; set; }

    public string Notes { get; set; } = string.Empty;
}