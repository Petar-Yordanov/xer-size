using XerSize.ViewModels.Statistics;

namespace XerSize.Views.Pages;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsPageViewModel _viewModel;
    private bool _isLoaded;
    private bool _isLoading;

    public StatisticsPage(StatisticsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded || _isLoading)
            return;

        _isLoading = true;

        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            _isLoading = false;
        }
    }
}