using XerSize.ViewModels.History;

namespace XerSize.Views.Pages;

public partial class HistorySessionDetailsPage : ContentPage
{
    HistorySessionDetailsPageViewModel _viewModel;

    public HistorySessionDetailsPage(HistorySessionDetailsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
