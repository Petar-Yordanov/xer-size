using XerSize.Models.Definitions;

namespace XerSize.Models.Presentation.Settings;

public static class ProfilePresentationOptions
{
    public static IReadOnlyList<GenderOption> Genders { get; } =
    [
        GenderOption.Male,
        GenderOption.Female
    ];

    public static IReadOnlyList<int> WeeklyGoalSessions { get; } =
    [
        1, 2, 3, 4, 5, 6, 7
    ];

    public static IReadOnlyList<UnitSystem> Units { get; } =
    [
        UnitSystem.Metric,
        UnitSystem.Imperial
    ];

    public static IReadOnlyList<InitialPageOption> InitialPages { get; } =
    [
        InitialPageOption.Dashboard,
        InitialPageOption.Workouts
    ];

    public static IReadOnlyList<AppThemeOption> Themes { get; } =
    [
        AppThemeOption.System,
        AppThemeOption.Light,
        AppThemeOption.Dark
    ];

    public static string ToDisplayName(GenderOption value)
    {
        return value switch
        {
            GenderOption.Male => "Male",
            GenderOption.Female => "Female",
            _ => "Male"
        };
    }

    public static string ToWeeklyGoalDisplayName(int sessions)
    {
        return sessions == 1
            ? "1 session"
            : $"{sessions} sessions";
    }

    public static string ToDisplayName(UnitSystem value)
    {
        return value switch
        {
            UnitSystem.Metric => "Metric",
            UnitSystem.Imperial => "Imperial",
            _ => "Metric"
        };
    }

    public static string ToDisplayName(InitialPageOption value)
    {
        return value switch
        {
            InitialPageOption.Dashboard => "Dashboard",
            InitialPageOption.Workouts => "Workouts",
            _ => "Dashboard"
        };
    }

    public static string ToDisplayName(AppThemeOption value)
    {
        return value switch
        {
            AppThemeOption.Light => "Light",
            AppThemeOption.Dark => "Dark",
            _ => "System"
        };
    }
}