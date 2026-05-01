using System.Globalization;

namespace XerSize.Models.Presentation.Common;

public static class PresentationFormatting
{
    public static string FormatPosition(int sortNumber)
    {
        return $"Position {sortNumber + 1}";
    }

    public static string FormatSetLabel(int sortNumber)
    {
        return $"Set {sortNumber + 1}";
    }

    public static string FormatOrdinalPrefix(int sortNumber)
    {
        return $"{sortNumber + 1}.";
    }

    public static string FormatReps(int reps)
    {
        return $"{Math.Max(0, reps)} reps";
    }

    public static string FormatWeightKg(double? weightKg)
    {
        return weightKg.HasValue
            ? $"{Math.Max(0, weightKg.Value).ToString("0.#", CultureInfo.InvariantCulture)} kg"
            : "No weight";
    }

    public static string FormatRestSeconds(int restSeconds)
    {
        var normalized = Math.Max(0, restSeconds);
        var minutes = normalized / 60;
        var seconds = normalized % 60;

        if (minutes <= 0)
            return $"{seconds} sec";

        if (seconds <= 0)
            return $"{minutes} min";

        return $"{minutes} min {seconds} sec";
    }

    public static string FormatVolumeKg(double volumeKg)
    {
        var normalized = Math.Max(0, volumeKg);

        return normalized >= 1000
            ? $"{normalized / 1000d:0.#}k kg"
            : $"{normalized:0.#} kg";
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

        return double.TryParse(
            normalized,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var result)
            ? Math.Max(0, result)
            : null;
    }

    public static double CalculateVolumeKg(int reps, double? weightKg)
    {
        return Math.Max(0, reps) * Math.Max(0, weightKg ?? 0);
    }
}