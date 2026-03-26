using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class CatalogExercisePickerPage : ContentPage
{
    private readonly CatalogExercisePickerPageViewModel _viewModel;

    public CatalogExercisePickerPage(CatalogExercisePickerPageViewModel viewModel)
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