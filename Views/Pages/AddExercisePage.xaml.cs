using XerSize.ViewModels.Routines;

namespace XerSize.Views.Pages;

public partial class AddExercisePage : ContentPage
{
    public AddExercisePage(AddExercisePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}