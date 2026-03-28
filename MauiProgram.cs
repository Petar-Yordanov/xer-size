using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using XerSize.Services;
using XerSize.Services.Interfaces;
using XerSize.ViewModels.Exercises;
using XerSize.ViewModels.History;
using XerSize.ViewModels.Routines;
using XerSize.ViewModels.Settings;
using XerSize.ViewModels.Statistics;
using XerSize.Views.Pages;

namespace XerSize;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeUISemibold");
            });

        builder.Services.AddTransient<AppShell>();

        builder.Services.AddSingleton<IExerciseCatalogService, InMemoryExerciseCatalogService>();
        builder.Services.AddSingleton<IRoutineService, InMemoryRoutineService>();
        builder.Services.AddSingleton<IWorkoutHistoryService, InMemoryWorkoutHistoryService>();
        builder.Services.AddSingleton<IStatisticsService, InMemoryStatisticsService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();

        builder.Services.AddTransient<RoutinesPageViewModel>();
        builder.Services.AddTransient<ExercisesPageViewModel>();
        builder.Services.AddTransient<HistoryPageViewModel>();
        builder.Services.AddTransient<StatisticsPageViewModel>();
        builder.Services.AddTransient<SettingsPageViewModel>();
        builder.Services.AddTransient<AddExercisePageViewModel>();
        builder.Services.AddTransient<CatalogExercisePickerPageViewModel>();
        builder.Services.AddTransient<RoutineEditorViewModel>();
        builder.Services.AddTransient<RoutineManagerPageViewModel>();
        builder.Services.AddTransient<WorkoutManagerPageViewModel>();

        builder.Services.AddTransient<RoutinesPage>();
        builder.Services.AddTransient<ExercisesPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<AddExercisePage>();
        builder.Services.AddTransient<CatalogExercisePickerPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<RoutineManagerPage>();
        builder.Services.AddTransient<WorkoutManagerPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}