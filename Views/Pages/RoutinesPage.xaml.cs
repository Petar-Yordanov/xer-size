using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class RoutinesPage : ContentPage
{
    private readonly RoutinesPageViewModel _viewModel;

    public RoutinesPage(RoutinesPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
