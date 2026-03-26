using XerSize.Resources.Themes;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class ThemeService : IThemeService
{
    private const string PreferenceKey = "app_theme";

    public string CurrentTheme { get; private set; } = "Light";

    public void Initialize()
    {
        var theme = Preferences.Default.Get(PreferenceKey, "Light");
        SetTheme(theme);
    }

    public void SetTheme(string themeName)
    {
        CurrentTheme = themeName == "Dark" ? "Dark" : "Light";
        Preferences.Default.Set(PreferenceKey, CurrentTheme);

        if (Application.Current is null)
            return;

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var existingThemes = mergedDictionaries
            .Where(x => x is LightTheme || x is DarkTheme)
            .ToList();

        foreach (var theme in existingThemes)
            mergedDictionaries.Remove(theme);

        mergedDictionaries.Add(CurrentTheme == "Dark"
            ? new DarkTheme()
            : new LightTheme());

        Application.Current.UserAppTheme =
            CurrentTheme == "Dark" ? AppTheme.Dark : AppTheme.Light;
    }
}