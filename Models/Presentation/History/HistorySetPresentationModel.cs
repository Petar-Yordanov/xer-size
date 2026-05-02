using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.History;

public sealed class HistorySetPresentationModel
{
    public Guid Id { get; set; }

    public Guid HistoryExerciseId { get; set; }

    public ExerciseTrackingMode TrackingMode { get; set; } = ExerciseTrackingMode.Strength;

    public int SortNumber { get; set; }

    public int Reps { get; set; }

    public double? WeightKg { get; set; }

    public int DurationSeconds { get; set; }

    public double? DistanceMeters { get; set; }

    public int RestSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsSkipped { get; set; }

    public DateTime? CompletedAt { get; set; }

    public double VolumeKg => IsCompleted && TrackingMode == ExerciseTrackingMode.Strength
        ? PresentationFormatting.CalculateVolumeKg(Reps, WeightKg)
        : 0;

    public string SortNumberText => PresentationFormatting.FormatSetLabel(SortNumber);

    public string SetText
    {
        get
        {
            var status = IsSkipped
                ? "Skipped"
                : IsCompleted
                    ? "Done"
                    : "Todo";

            return TrackingMode switch
            {
                ExerciseTrackingMode.Time => $"{SortNumberText}: {FormatDuration(DurationSeconds)} • {status}",
                ExerciseTrackingMode.TimeAndDistance => $"{SortNumberText}: {FormatDuration(DurationSeconds)} • {FormatDistance(DistanceMeters)} • {status}",
                _ => BuildStrengthText(status)
            };
        }
    }

    private string BuildStrengthText(string status)
    {
        if (WeightKg.HasValue && WeightKg.Value > 0)
            return $"{SortNumberText}: {Reps} × {PresentationFormatting.FormatWeightKg(WeightKg)} • {status}";

        return $"{SortNumberText}: {PresentationFormatting.FormatReps(Reps)} • {status}";
    }

    private static string FormatDuration(int seconds)
    {
        seconds = Math.Max(0, seconds);

        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;

        return minutes > 0
            ? $"{minutes}:{remainingSeconds:00}"
            : $"{remainingSeconds}s";
    }

    private static string FormatDistance(double? meters)
    {
        if (!meters.HasValue || meters.Value <= 0)
            return "0 km";

        return meters.Value >= 1000
            ? $"{meters.Value / 1000d:0.##} km"
            : $"{meters.Value:0} m";
    }
}