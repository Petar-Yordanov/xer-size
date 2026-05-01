using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;
using XerSize.Repositories.ActiveWorkout;
using XerSize.Repositories.Catalog;
using XerSize.Repositories.Common;
using XerSize.Repositories.History;
using XerSize.Repositories.Settings;
using XerSize.Repositories.Workouts;
using XerSize.Services;
using XerSize.ViewModels;
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
            });

        RegisterRepositories(builder.Services);
        RegisterServices(builder.Services);
        RegisterViewModels(builder.Services);
        RegisterPages(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });
#endif

        var app = builder.Build();

        return app;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddSingleton<SqliteLocalStore>();

        services.AddSingleton<WorkoutRepository>();
        services.AddSingleton<WorkoutExerciseItemRepository>();
        services.AddSingleton<WorkoutSetRepository>();

        services.AddSingleton<ExerciseCatalogItemRepository>();

        services.AddSingleton<HistoryWorkoutItemRepository>();
        services.AddSingleton<HistoryExerciseItemRepository>();
        services.AddSingleton<HistorySetItemRepository>();

        services.AddSingleton<ActiveWorkoutSessionRepository>();
        services.AddSingleton<ActiveWorkoutExerciseRepository>();
        services.AddSingleton<ActiveWorkoutSetRepository>();

        services.AddSingleton<UserSettingsRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<WorkoutService>();
        services.AddSingleton<ExerciseCatalogService>();
        services.AddSingleton<WorkoutSelectionService>();
        services.AddSingleton<DashboardStatisticsService>();
        services.AddSingleton<WorkoutHistoryService>();
        services.AddSingleton<UserSettingsService>();
        services.AddSingleton<ActiveWorkoutService>();
        services.AddSingleton<TrainingAdviceService>();
        services.AddSingleton(AudioManager.Current);
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<DashboardPageViewModel>();
        services.AddTransient<WorkoutsPageViewModel>();
        services.AddTransient<HistoryPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<ManageWorkoutsPageViewModel>();
        services.AddTransient<AddExercisePageViewModel>();
        services.AddTransient<CatalogExercisePickerPageViewModel>();
        services.AddTransient<StartWorkoutPageViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<DashboardPage>();
        services.AddTransient<WorkoutsPage>();
        services.AddTransient<HistoryPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<ManageWorkoutsPage>();
        services.AddTransient<AddExercisePage>();
        services.AddTransient<CatalogExercisePickerPage>();
        services.AddTransient<StartWorkoutPage>();
    }
}