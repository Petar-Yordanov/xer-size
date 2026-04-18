using XerSize.ViewModels.Workouts;

namespace XerSize.Views.Pages;

public partial class ActiveWorkoutPage : ContentPage
{
    private readonly ActiveWorkoutPageViewModel _viewModel;
    private bool _isLoading;

    public ActiveWorkoutPage(ActiveWorkoutPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
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