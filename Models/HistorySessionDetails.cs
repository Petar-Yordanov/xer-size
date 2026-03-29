using CommunityToolkit.Mvvm.ComponentModel;

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

    public List<HistoryDetailStatChip> SummaryChips { get; set; } = new();
    public List<HistoryDetailComparisonChip> WorkoutComparisonChips { get; set; } = new();
    public List<HistoryDetailExerciseRow> Exercises { get; set; } = new();
}

public partial class HistoryDetailExerciseRow : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string MetaText { get; set; } = string.Empty;

    public List<HistoryDetailStatChip> StatChips { get; set; } = new();
    public List<HistoryDetailComparisonChip> ComparisonChips { get; set; } = new();
    public List<HistoryDetailSetRow> Sets { get; set; } = new();

    [ObservableProperty]
    public partial bool IsSetsExpanded { get; set; }

    public string ToggleSetsText => IsSetsExpanded ? "Hide info" : "Show info";

    partial void OnIsSetsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleSetsText));
    }
}

public sealed class HistoryDetailSetRow
{
    public string Text { get; set; } = string.Empty;
}

public sealed class HistoryDetailStatChip
{
    public string Text { get; set; } = string.Empty;
}

public sealed class HistoryDetailComparisonChip
{
    public string Text { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public bool IsNegative { get; set; }
}