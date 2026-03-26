namespace XerSize.Models;

public sealed class HistorySessionDetails
{
    public Guid SessionId { get; set; }

    public string RoutineName { get; set; } = string.Empty;
    public string WorkoutName { get; set; } = string.Empty;

    public DateTime PerformedAt { get; set; }
    public int DurationMinutes { get; set; }
    public double EstimatedCaloriesBurned { get; set; }
    public double BodyWeightKg { get; set; }

    public double TotalVolume { get; set; }
    public int TotalSets { get; set; }
    public int TotalReps { get; set; }

    public List<HistoryDetailExerciseRow> Exercises { get; set; } = new();
}

public sealed class HistoryDetailExerciseRow
{
    public string Name { get; set; } = string.Empty;
    public string MetaText { get; set; } = string.Empty;
    public string VolumeText { get; set; } = string.Empty;
    public List<HistoryDetailSetRow> Sets { get; set; } = new();
}

public sealed class HistoryDetailSetRow
{
    public string Text { get; set; } = string.Empty;
}