using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
        : this(ResolveViewModel())
    {
    }

    public HistoryPage(HistoryPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HistoryPageViewModel vm)
            vm.SyncSelectedNav();
    }

    private static HistoryPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<HistoryPageViewModel>()
            ?? throw new InvalidOperationException("HistoryPageViewModel is not registered in the service provider.");
    }
}