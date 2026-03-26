using XerSize.ViewModels.Exercises;

namespace XerSize.Views.Pages;

public partial class ExercisesPage : ContentPage
{
    ExercisesPageViewModel _viewModel;

    public ExercisesPage(ExercisesPageViewModel viewModel)
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
            throw;
        }
    }
}
