using XerSize.Views.Pages;

namespace XerSize;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(AddExercisePage), typeof(AddExercisePage));
        Routing.RegisterRoute(nameof(CatalogExercisePickerPage), typeof(CatalogExercisePickerPage));
        Routing.RegisterRoute(nameof(RoutineManagerPage), typeof(RoutineManagerPage));
        Routing.RegisterRoute(nameof(WorkoutManagerPage), typeof(WorkoutManagerPage));
    }
}