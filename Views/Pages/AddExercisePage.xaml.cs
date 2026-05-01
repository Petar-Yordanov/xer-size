using Microsoft.Extensions.DependencyInjection;
using XerSize.ViewModels;

namespace XerSize.Views.Pages;

public partial class AddExercisePage : ContentPage, IQueryAttributable
{
    private bool hasAppliedInitialMode;

    public AddExercisePage()
        : this(ResolveViewModel())
    {
    }

    public AddExercisePage(AddExercisePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is not AddExercisePageViewModel vm)
            return;

        if (query.TryGetValue("exerciseId", out var exerciseIdValue) &&
            Guid.TryParse(exerciseIdValue?.ToString(), out var workoutExerciseId))
        {
            vm.LoadForEdit(workoutExerciseId);
            hasAppliedInitialMode = true;
            return;
        }

        if (query.TryGetValue("catalogExerciseId", out var catalogExerciseIdValue))
        {
            var catalogExerciseId = catalogExerciseIdValue?.ToString();

            if (!string.IsNullOrWhiteSpace(catalogExerciseId))
                vm.ApplyCatalogExercise(catalogExerciseId);

            hasAppliedInitialMode = true;
            return;
        }

        if (!hasAppliedInitialMode)
        {
            vm.LoadForCreate();
            hasAppliedInitialMode = true;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AddExercisePageViewModel vm && !hasAppliedInitialMode)
        {
            vm.LoadForCreate();
            hasAppliedInitialMode = true;
        }
    }

    private static AddExercisePageViewModel ResolveViewModel()
    {
        return IPlatformApplication.Current?.Services.GetRequiredService<AddExercisePageViewModel>()
            ?? throw new InvalidOperationException("AddExercisePageViewModel is not registered in the service provider.");
    }
}