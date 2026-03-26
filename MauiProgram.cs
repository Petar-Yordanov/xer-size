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

        builder.Services.AddSingleton<IExerciseCatalogService, InMemoryExerciseCatalogService>();
        builder.Services.AddSingleton<IRoutineService, InMemoryRoutineService>();
        builder.Services.AddSingleton<IWorkoutHistoryService, InMemoryWorkoutHistoryService>();
        builder.Services.AddSingleton<IStatisticsService, InMemoryStatisticsService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddTransient<HistorySessionDetailsPageViewModel>();
        builder.Services.AddTransient<HistorySessionDetailsPage>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddSingleton<RoutinesPageViewModel>();
        builder.Services.AddSingleton<ExercisesPageViewModel>();
        builder.Services.AddSingleton<HistoryPageViewModel>();
        builder.Services.AddTransient<HistorySessionDetailsPageViewModel>();
        builder.Services.AddSingleton<StatisticsPageViewModel>();
        builder.Services.AddSingleton<SettingsPageViewModel>();
        builder.Services.AddTransient<AddExercisePageViewModel>();
        builder.Services.AddTransient<CatalogExercisePickerPageViewModel>();
        builder.Services.AddTransient<RoutineEditorViewModel>();

        builder.Services.AddSingleton<RoutinesPage>();
        builder.Services.AddSingleton<ExercisesPage>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddTransient<HistorySessionDetailsPage>();
        builder.Services.AddSingleton<StatisticsPage>();
        builder.Services.AddTransient<AddExercisePage>();
        builder.Services.AddTransient<CatalogExercisePickerPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}