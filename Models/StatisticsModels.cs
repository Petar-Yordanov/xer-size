namespace XerSize.Models;

public enum StatisticsTab
{
    Preferences,
    TimelineData
}

public enum StatisticsTimelineBucket
{
    Days,
    Weeks,
    Months
}

public enum StatisticsQuarter
{
    Q1 = 1,
    Q2 = 2,
    Q3 = 3,
    Q4 = 4
}

public sealed class TrendPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class TrendSeries
{
    public string Label { get; set; } = string.Empty;
    public List<TrendPoint> Points { get; set; } = new();
}

public sealed class BreakdownItem
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Percent { get; set; }
}

public sealed class PlateauInsight
{
    public bool IsPlateaued { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public sealed class StatisticsSnapshot
{
    public string Title { get; set; } = string.Empty;
    public bool HasSelection { get; set; }
    public bool HasWorkoutSelection { get; set; }

    public string SelectionSummary { get; set; } = "Select routine, then select the workout.";

    // Preferences
    public int CurrentWorkoutCount { get; set; }
    public int CurrentExerciseCount { get; set; }
    public int CurrentTotalSets { get; set; }
    public double CurrentPlannedVolume { get; set; }

    public List<BreakdownItem> MuscleGroupPreference { get; set; } = new();
    public List<BreakdownItem> EquipmentPreference { get; set; } = new();
    public List<BreakdownItem> MechanicPreference { get; set; } = new();
    public List<BreakdownItem> ExerciseVolumePreference { get; set; } = new();

    // Timeline
    public int TimelineWorkoutCount { get; set; }
    public int TimelineTotalSets { get; set; }
    public double TimelineTotalVolume { get; set; }
    public double TimelineEstimatedCaloriesBurned { get; set; }

    public List<TrendPoint> VolumeTrend { get; set; } = new();
    public List<TrendPoint> LiftedWeightTrend { get; set; } = new();
    public List<TrendPoint> GymVisitFrequencyTrend { get; set; } = new();
    public List<TrendSeries> ExerciseVolumeTrendSeries { get; set; } = new();

    public string GymVisitFrequencyTitle { get; set; } = "Gym Visits";
    public PlateauInsight Plateau { get; set; } = new();
}