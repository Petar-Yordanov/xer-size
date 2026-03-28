using XerSize.Services.Interfaces;

namespace XerSize;

public partial class App : Application
{
    public App(IThemeService themeService)
    {
        InitializeComponent();

        try
        {
            themeService.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}