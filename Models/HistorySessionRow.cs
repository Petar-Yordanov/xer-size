using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace XerSize.Models;

public partial class HistorySessionRow : ObservableObject
{
    [ObservableProperty]
    public partial Guid SessionId { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Subtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DurationText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CaloriesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BodyWeightText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VolumeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SetsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RepsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VolumeDeltaText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RepsDeltaText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsVolumeUp { get; set; }

    [ObservableProperty]
    public partial bool IsVolumeDown { get; set; }

    [ObservableProperty]
    public partial bool IsRepsUp { get; set; }

    [ObservableProperty]
    public partial bool IsRepsDown { get; set; }

    [ObservableProperty]
    public partial bool IsInfoExpanded { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<HistorySessionExerciseInfoRow> ExerciseInfos { get; set; } =
        Array.Empty<HistorySessionExerciseInfoRow>();

    public string ToggleInfoText => IsInfoExpanded ? "Hide Info" : "Show Info";

    partial void OnIsInfoExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleInfoText));
    }

    [RelayCommand]
    private void ToggleInfo()
    {
        IsInfoExpanded = !IsInfoExpanded;
    }
}

public partial class HistorySessionExerciseInfoRow : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SummaryText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<HistorySessionSetInfoRow> Sets { get; set; } =
        Array.Empty<HistorySessionSetInfoRow>();
}

public partial class HistorySessionSetInfoRow : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<SetPartItem> Parts { get; set; } = Array.Empty<SetPartItem>();
}