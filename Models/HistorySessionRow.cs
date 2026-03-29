using CommunityToolkit.Mvvm.ComponentModel;

namespace XerSize.Models;

public partial class HistorySessionRow : ObservableObject
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

    public List<HistorySessionExerciseInfoRow> ExerciseInfos { get; set; } = new();

    [ObservableProperty]
    public partial bool IsInfoExpanded { get; set; }

    public string ToggleInfoText => IsInfoExpanded ? "Hide info" : "Show info";

    partial void OnIsInfoExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleInfoText));
    }
}

public sealed class HistorySessionExerciseInfoRow
{
    public string Name { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public List<HistorySessionSetInfoRow> Sets { get; set; } = new();
}

public sealed class HistorySessionSetInfoRow
{
    public string Text { get; set; } = string.Empty;
}