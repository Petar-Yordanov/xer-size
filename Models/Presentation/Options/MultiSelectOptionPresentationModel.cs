using CommunityToolkit.Mvvm.ComponentModel;

namespace XerSize.Models.Presentation.Options;

public partial class MultiSelectOptionPresentationModel : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    public string Name { get; set; } = string.Empty;
}