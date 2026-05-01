namespace XerSize.Models.Presentation.Navigation;

public sealed class BottomNavItemPresentationModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string IconSource { get; set; } = string.Empty;

    public string Route { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    public string BadgeText { get; set; } = string.Empty;

    public bool HasBadge => !string.IsNullOrWhiteSpace(BadgeText);
}