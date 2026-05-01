using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
        : this(ResolveViewModel())
    {
    }

    public DashboardPage(DashboardPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardPageViewModel vm)
            vm.SyncSelectedNav();
    }

    private static DashboardPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<DashboardPageViewModel>()
            ?? throw new InvalidOperationException("DashboardPageViewModel is not registered in the service provider.");
    }
}