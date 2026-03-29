namespace XerSize.Models;

public sealed class Exercise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}
