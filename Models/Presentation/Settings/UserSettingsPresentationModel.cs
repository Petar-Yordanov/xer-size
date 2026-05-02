using XerSize.Models.Definitions;

namespace XerSize.Models.Presentation.Settings;

public sealed class UserSettingsPresentationModel
{
    public Guid Id { get; set; }

    public string Age { get; set; } = string.Empty;

    public string Height { get; set; } = string.Empty;

    public string Weight { get; set; } = string.Empty;

    public GenderOption Gender { get; set; } = GenderOption.Male;

    public int WeeklyGoalSessions { get; set; } = 3;

    public int WeeklyCalorieTarget { get; set; } = 2000;

    public UnitSystem Units { get; set; } = UnitSystem.Metric;

    public InitialPageOption InitialPage { get; set; } = InitialPageOption.Dashboard;

    public AppThemeOption Theme { get; set; } = AppThemeOption.System;

    public bool KeepScreenAwakeDuringWorkout { get; set; } = true;

    public bool AutoExpandExerciseCards { get; set; } = true;

    public bool RestTimerSoundEnabled { get; set; } = true;

    public string GenderText => ProfilePresentationOptions.ToDisplayName(Gender);

    public string WeeklyGoalText => ProfilePresentationOptions.ToWeeklyGoalDisplayName(WeeklyGoalSessions);

    public string WeeklyCalorieTargetText => $"{Math.Max(0, WeeklyCalorieTarget)} kcal";

    public string UnitsText => ProfilePresentationOptions.ToDisplayName(Units);

    public string InitialPageText => ProfilePresentationOptions.ToDisplayName(InitialPage);

    public string ThemeText => ProfilePresentationOptions.ToDisplayName(Theme);
}