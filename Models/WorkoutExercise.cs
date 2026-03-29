namespace XerSize.Models;

public sealed class WorkoutExercise
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CatalogExerciseId { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Force { get; set; } = string.Empty;
    public string BodyCategory { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public List<string> PrimaryMuscles { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public List<string> PrimaryMuscleCategories { get; set; } = new();
    public List<string> SecondaryMuscleCategories { get; set; } = new();
    public string LimbInvolvement { get; set; } = string.Empty;
    public string? MovementPattern { get; set; }

    public int? DefaultRestSeconds { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }

    public List<WorkoutExerciseSet> Sets { get; set; } = new();
}