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
            return existingSettings;

        return userSettings.Create(new UserSettingsModel());
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
        settings.Gender = gender;
        settings.WeeklyGoalSessions = Math.Clamp(weeklyGoalSessions, 1, 7);
        settings.WeeklyCalorieTarget = Math.Max(0, weeklyCalorieTarget);
        settings.Units = units;
        settings.InitialPage = initialPage;

        userSettings.Update(settings.Id, settings);
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
}