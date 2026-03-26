namespace XerSize.Models;

public enum StatisticsRange
{
    LastWeek,
    Last2Weeks,
    LastMonth,
    Last3Months,
    Last6Months,
    LastYear
}

public enum StatisticsScopeKind
{
    Routine,
    Workout
}

public sealed class TrendPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Label => Date.ToString("dd MMM");
}

public sealed class BreakdownItem
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Percent { get; set; }
}

public sealed class CalendarDayItem
{
    public DateTime Date { get; set; }
    public bool WorkedOut { get; set; }
    public double Volume { get; set; }
}

public sealed class PlateauInsight
{
    public bool IsPlateaued { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public sealed class StatisticsSnapshot
{
    public string Title { get; set; } = string.Empty;

    public int WorkoutCount { get; set; }
    public int RestDays { get; set; }
    public int MuscleGroupsHitCount { get; set; }
    public int TotalSets { get; set; }
    public double TotalVolume { get; set; }
    public double EstimatedCaloriesBurned { get; set; }

    public List<TrendPoint> VolumeTrend { get; set; } = new();
    public List<TrendPoint> WeightTrend { get; set; } = new();

    public List<BreakdownItem> SetsPerMuscleGroup { get; set; } = new();
    public List<BreakdownItem> EquipmentPreference { get; set; } = new();
    public List<BreakdownItem> MechanicPreference { get; set; } = new();
    public List<BreakdownItem> LateralityPreference { get; set; } = new();
    public List<BreakdownItem> NeglectedMuscles { get; set; } = new();

    public PlateauInsight Plateau { get; set; } = new();
    public List<CalendarDayItem> Calendar { get; set; } = new();
}