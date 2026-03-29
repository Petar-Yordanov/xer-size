using CommunityToolkit.Mvvm.ComponentModel;

namespace XerSize.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }
}
