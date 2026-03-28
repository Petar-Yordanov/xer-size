using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class WorkoutManagerPage : ContentPage
{
    private readonly WorkoutManagerPageViewModel _viewModel;

    public WorkoutManagerPage(WorkoutManagerPageViewModel viewModel)
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