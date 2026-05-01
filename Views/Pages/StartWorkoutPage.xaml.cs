using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class StartWorkoutPage : ContentPage
{
    public StartWorkoutPage()
        : this(ResolveViewModel())
    {
    }

    public StartWorkoutPage(StartWorkoutPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is StartWorkoutPageViewModel vm && Dispatcher is not null)
            vm.Initialize(Dispatcher);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is StartWorkoutPageViewModel vm)
            vm.StopTimers();
    }

    private static StartWorkoutPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<StartWorkoutPageViewModel>()
            ?? throw new InvalidOperationException("StartWorkoutPageViewModel is not registered in the service provider.");
    }
}