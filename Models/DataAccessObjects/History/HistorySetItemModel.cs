namespace XerSize.Models.DataAccessObjects.History;

public sealed class HistorySetItemModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HistoryExerciseId { get; set; }

    public int SortNumber { get; set; }

    public int Reps { get; set; }

    public double? WeightKg { get; set; }

    public int RestSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsSkipped { get; set; }

    public DateTime? CompletedAt { get; set; }
}