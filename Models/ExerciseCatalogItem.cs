namespace XerSize.Models;

public sealed class ExerciseCatalogItem
{
    public string Id { get; set; } = string.Empty;
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
}