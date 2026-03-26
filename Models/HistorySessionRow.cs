namespace XerSize.Models;

public sealed class HistorySessionRow
{
    public Guid SessionId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;

    public string DateText { get; set; } = string.Empty;
    public string DurationText { get; set; } = string.Empty;
    public string CaloriesText { get; set; } = string.Empty;
    public string BodyWeightText { get; set; } = string.Empty;

    public string VolumeText { get; set; } = string.Empty;
    public string SetsText { get; set; } = string.Empty;
    public string RepsText { get; set; } = string.Empty;

    public string VolumeDeltaText { get; set; } = string.Empty;
    public string RepsDeltaText { get; set; } = string.Empty;

    public bool IsVolumeUp { get; set; }
    public bool IsVolumeDown { get; set; }
    public bool IsRepsUp { get; set; }
    public bool IsRepsDown { get; set; }
}