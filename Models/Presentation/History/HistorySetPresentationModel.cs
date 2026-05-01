using XerSize.Models.Presentation.Common;

namespace XerSize.Models.Presentation.History;

public sealed class HistorySetPresentationModel
{
    public Guid Id { get; set; }

    public Guid HistoryExerciseId { get; set; }

    public int SortNumber { get; set; }

    public int Reps { get; set; }

    public double? WeightKg { get; set; }

    public int RestSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsSkipped { get; set; }

    public DateTime? CompletedAt { get; set; }

    public double VolumeKg => IsCompleted
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

            if (WeightKg.HasValue && WeightKg.Value > 0)
                return $"{SortNumberText}: {Reps} × {PresentationFormatting.FormatWeightKg(WeightKg)} • {status}";

            return $"{SortNumberText}: {PresentationFormatting.FormatReps(Reps)} • {status}";
        }
    }
}