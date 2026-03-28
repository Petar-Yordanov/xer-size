using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class RoutineManagerPage : ContentPage
{
    private readonly RoutineManagerPageViewModel _viewModel;

    public RoutineManagerPage(RoutineManagerPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}