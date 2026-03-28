using XerSize.ViewModels.Statistics;

namespace XerSize.Views.Pages;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsPageViewModel _viewModel;

    public StatisticsPage(StatisticsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.OnPageAppearingAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}