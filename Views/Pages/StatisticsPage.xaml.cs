using XerSize.ViewModels.Statistics;

namespace XerSize.Views.Pages;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsPageViewModel _viewModel;
    private bool _hasLoaded;

    public StatisticsPage(StatisticsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
            return;

        _hasLoaded = true;
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}