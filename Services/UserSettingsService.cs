using XerSize.Models.DataAccessObjects.Settings;
using XerSize.Models.Definitions;
using XerSize.Repositories.Settings;

namespace XerSize.Services;

public sealed class UserSettingsService
{
    private readonly UserSettingsRepository userSettings;

    public UserSettingsService(UserSettingsRepository userSettings)
    {
        this.userSettings = userSettings;
    }

    public bool Exists()
    {
        return userSettings.Count() > 0;
    }

    public UserSettingsModel GetOrCreate()
    {
        var existingSettings = userSettings.Get().FirstOrDefault();

        if (existingSettings is not null)
        {
            NormalizeSettings(existingSettings);
            userSettings.Update(existingSettings.Id, existingSettings);
            ApplyTheme(existingSettings.Theme);

            return existingSettings;
        }

        var createdSettings = userSettings.Create(new UserSettingsModel());
        ApplyTheme(createdSettings.Theme);

        return createdSettings;
    }

    public void UpdateProfile(
        string age,
        string height,
        string weight,
        GenderOption gender,
        int weeklyGoalSessions,
        int weeklyCalorieTarget,
        UnitSystem units,
        InitialPageOption initialPage)
    {
        var settings = GetOrCreate();

        settings.Age = age.Trim();
        settings.Height = height.Trim();
        settings.Weight = weight.Trim();
        settings.Gender = NormalizeGender(gender);
        settings.WeeklyGoalSessions = Math.Clamp(weeklyGoalSessions, 1, 7);
        settings.WeeklyCalorieTarget = Math.Max(0, weeklyCalorieTarget);
        settings.Units = units;
        settings.InitialPage = initialPage;

        userSettings.Update(settings.Id, settings);
    }

    public void UpdateAppearance(AppThemeOption theme)
    {
        var settings = GetOrCreate();

        settings.Theme = NormalizeTheme(theme);

        userSettings.Update(settings.Id, settings);
        ApplyTheme(settings.Theme);
    }

    public void UpdatePreferences(
        bool keepScreenAwakeDuringWorkout,
        bool autoExpandExerciseCards,
        bool restTimerSoundEnabled)
    {
        var settings = GetOrCreate();

        settings.KeepScreenAwakeDuringWorkout = keepScreenAwakeDuringWorkout;
        settings.AutoExpandExerciseCards = autoExpandExerciseCards;
        settings.RestTimerSoundEnabled = restTimerSoundEnabled;

        userSettings.Update(settings.Id, settings);
    }

    public bool KeepScreenAwakeDuringWorkout()
    {
        return GetOrCreate().KeepScreenAwakeDuringWorkout;
    }

    public bool AutoExpandExerciseCards()
    {
        return GetOrCreate().AutoExpandExerciseCards;
    }

    public bool RestTimerSoundEnabled()
    {
        return GetOrCreate().RestTimerSoundEnabled;
    }

    public InitialPageOption InitialPage()
    {
        return GetOrCreate().InitialPage;
    }

    public AppThemeOption Theme()
    {
        return GetOrCreate().Theme;
    }

    private static void ApplyTheme(AppThemeOption theme)
    {
        if (Application.Current is null)
            return;

        Application.Current.UserAppTheme = NormalizeTheme(theme) switch
        {
            AppThemeOption.Light => AppTheme.Light,
            AppThemeOption.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    private static void NormalizeSettings(UserSettingsModel settings)
    {
        settings.Gender = NormalizeGender(settings.Gender);
        settings.WeeklyGoalSessions = Math.Clamp(settings.WeeklyGoalSessions, 1, 7);
        settings.WeeklyCalorieTarget = Math.Max(0, settings.WeeklyCalorieTarget);
        settings.Theme = NormalizeTheme(settings.Theme);
    }

    private static GenderOption NormalizeGender(GenderOption gender)
    {
        return gender == GenderOption.Female
            ? GenderOption.Female
            : GenderOption.Male;
    }

    private static AppThemeOption NormalizeTheme(AppThemeOption theme)
    {
        return theme switch
        {
            AppThemeOption.Light => AppThemeOption.Light,
            AppThemeOption.Dark => AppThemeOption.Dark,
            _ => AppThemeOption.System
        };
    }
}