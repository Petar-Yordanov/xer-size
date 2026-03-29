namespace XerSize.Services.Interfaces;

public interface IThemeService
{
    string CurrentTheme { get; }
    void Initialize();
    void SetTheme(string themeName);
}
