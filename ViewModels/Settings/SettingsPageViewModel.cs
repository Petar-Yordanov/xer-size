using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.Services.Interfaces;
using XerSize.ViewModels.Base;

namespace XerSize.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    public partial string CurrentTheme { get; set; } = "Light";

    public SettingsPageViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        Title = "Settings";
        CurrentTheme = _themeService.CurrentTheme;
    }

    [RelayCommand]
    private void ApplyLightTheme()
    {
        _themeService.SetTheme("Light");
        CurrentTheme = _themeService.CurrentTheme;
    }

    [RelayCommand]
    private void ApplyDarkTheme()
    {
        _themeService.SetTheme("Dark");
        CurrentTheme = _themeService.CurrentTheme;
    }
}
