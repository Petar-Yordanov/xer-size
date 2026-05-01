using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class CatalogExercisePickerPage : ContentPage
{
    public CatalogExercisePickerPage()
        : this(ResolveViewModel())
    {
    }

    public CatalogExercisePickerPage(CatalogExercisePickerPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CatalogExercisePickerPageViewModel vm)
            await vm.InitializeAsync();
    }

    private static CatalogExercisePickerPageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<CatalogExercisePickerPageViewModel>()
            ?? throw new InvalidOperationException("CatalogExercisePickerPageViewModel is not registered in the service provider.");
    }
}