using XerSize.Models.Presentation.ExerciseMetadata;

namespace XerSize.Models.Presentation.Catalog;

public sealed class ExerciseCatalogItemPresentationModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ImageSource { get; set; } = string.Empty;

    public ExerciseMetadataPresentationModel Metadata { get; set; } = new();

    public List<string> Aliases { get; set; } = [];

    public string SearchText => string.Join(
        " ",
        new[]
        {
            Id,
            Name,
            Notes,
            Metadata.SearchText,
            string.Join(" ", Aliases)
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
}