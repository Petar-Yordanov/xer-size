using Microsoft.Extensions.DependencyInjection;
using XerSize.Models.Definitions;
using XerSize.Services;

namespace XerSize
{
    public partial class AppShell : Shell
    {
        public const string DashboardRoute = "dashboard";
        public const string WorkoutsRoute = "workouts";
        public const string HistoryRoute = "history";
        public const string SettingsRoute = "settings";
        public const string ManageWorkoutsRoute = "manage-workouts";
        public const string AddExerciseRoute = "add-exercise";
        public const string CatalogExercisePickerRoute = "catalog-exercise-picker";
        public const string StartWorkoutRoute = "start-workout";

        private bool initialNavigationApplied;

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(WorkoutsRoute, typeof(Views.Pages.WorkoutsPage));
            Routing.RegisterRoute(HistoryRoute, typeof(Views.Pages.HistoryPage));
            Routing.RegisterRoute(SettingsRoute, typeof(Views.Pages.SettingsPage));
            Routing.RegisterRoute(ManageWorkoutsRoute, typeof(Views.Pages.ManageWorkoutsPage));
            Routing.RegisterRoute(AddExerciseRoute, typeof(Views.Pages.AddExercisePage));
            Routing.RegisterRoute(CatalogExercisePickerRoute, typeof(Views.Pages.CatalogExercisePickerPage));
            Routing.RegisterRoute(StartWorkoutRoute, typeof(Views.Pages.StartWorkoutPage));

            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            if (initialNavigationApplied)
                return;

            initialNavigationApplied = true;

            Dispatcher.Dispatch(async () => await ApplyInitialPageAsync());
        }

        private static async Task ApplyInitialPageAsync()
        {
            var userSettingsService = IPlatformApplication.Current?.Services.GetService<UserSettingsService>();

            if (userSettingsService is null)
                return;

            var initialPage = userSettingsService.InitialPage();

            if (initialPage == InitialPageOption.Dashboard)
                return;

            if (initialPage == InitialPageOption.Workouts)
                await Shell.Current.GoToAsync(WorkoutsRoute, false);
        }
    }
}