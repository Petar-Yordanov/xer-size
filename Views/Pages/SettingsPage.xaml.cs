using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
        : this(ResolveViewModel())
    {
    }

    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SettingsPageViewModel vm)
            vm.SyncSelectedNav();
    }

    private static SettingsPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<SettingsPageViewModel>()
            ?? throw new InvalidOperationException("SettingsPageViewModel is not registered in the service provider.");
    }
}