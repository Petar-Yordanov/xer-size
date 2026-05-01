using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class WorkoutsPage : ContentPage
{
    public WorkoutsPage()
        : this(ResolveViewModel())
    {
    }

    public WorkoutsPage(WorkoutsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is WorkoutsPageViewModel vm)
        {
            vm.SyncSelectedNav();
            vm.RefreshSelectedWorkout();
        }
    }

    private static WorkoutsPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<WorkoutsPageViewModel>()
            ?? throw new InvalidOperationException("WorkoutsPageViewModel is not registered in the service provider.");
    }
}