using System.Globalization;

namespace XerSize.Models.Presentation.Common;

public static class PresentationFormatting
{
    public static string FormatSetLabel(int sortNumber)
    {
        return $"Set {Math.Max(0, sortNumber) + 1}";
    }

    public static string FormatPosition(int sortNumber)
    {
        return (Math.Max(0, sortNumber) + 1).ToString(CultureInfo.InvariantCulture);
    }

    public static string FormatOrdinalPrefix(int sortNumber)
    {
        var value = Math.Max(0, sortNumber) + 1;

        return value switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            _ => $"{value}th"
        };
    }

    public static int ParseNonNegativeInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return int.TryParse(
            value.Trim(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result)
            ? Math.Max(0, result)
            : 0;
    }

    public static double? ParseNonNegativeNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().Replace(',', '.');

        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantResult))
            return Math.Max(0, invariantResult);

        if (double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out var currentResult))
            return Math.Max(0, currentResult);

        return null;
    }

    public static string FormatReps(int reps)
    {
        return reps == 1
            ? "1 rep"
            : $"{Math.Max(0, reps)} reps";
    }

    public static string FormatWeightKg(double? weightKg)
    {
        if (!weightKg.HasValue || weightKg.Value <= 0)
            return "Bodyweight";

        return $"{weightKg.Value:0.#} kg";
    }

    public static string FormatRestSeconds(int restSeconds)
    {
        var seconds = Math.Max(0, restSeconds);

        if (seconds <= 0)
            return "No rest";

        if (seconds < 60)
            return $"{seconds}s";

        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;

        return remainingSeconds == 0
            ? $"{minutes}m"
            : $"{minutes}m {remainingSeconds}s";
    }

    public static string FormatDurationSeconds(int durationSeconds)
    {
        var seconds = Math.Max(0, durationSeconds);

        if (seconds <= 0)
            return "No time";

        if (seconds < 60)
            return $"{seconds}s";

        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;

        if (minutes < 60)
        {
            return remainingSeconds == 0
                ? $"{minutes}m"
                : $"{minutes}m {remainingSeconds}s";
        }

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return remainingMinutes == 0
            ? $"{hours}h"
            : $"{hours}h {remainingMinutes}m";
    }

    public static string FormatDistanceMeters(double? meters)
    {
        if (!meters.HasValue || meters.Value <= 0)
            return "0 km";

        return meters.Value >= 1000
            ? $"{meters.Value / 1000d:0.##} km"
            : $"{meters.Value:0} m";
    }

    public static string FormatVolumeKg(double volumeKg)
    {
        return volumeKg <= 0
            ? "0 kg"
            : $"{volumeKg:0.#} kg";
    }

    public static double CalculateVolumeKg(int reps, double? weightKg)
    {
        return Math.Max(0, reps) * Math.Max(0, weightKg ?? 0);
    }
}