using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class ManageWorkoutsPage : ContentPage
{
    public ManageWorkoutsPage()
        : this(ResolveViewModel())
    {
    }

    public ManageWorkoutsPage(ManageWorkoutsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private static ManageWorkoutsPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<ManageWorkoutsPageViewModel>()
            ?? throw new InvalidOperationException("ManageWorkoutsPageViewModel is not registered in the service provider.");
    }
}