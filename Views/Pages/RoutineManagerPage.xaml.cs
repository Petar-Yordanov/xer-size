using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class RoutineManagerPage : ContentPage
{
    private readonly RoutineManagerPageViewModel _viewModel;
    private bool _isLoaded;
    private bool _isLoading;

    public RoutineManagerPage(RoutineManagerPageViewModel viewModel)
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