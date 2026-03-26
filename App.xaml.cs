using XerSize.Services.Interfaces;

namespace XerSize;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell, IThemeService themeService)
    {
        InitializeComponent();
        _shell = shell;

        try
        {
            themeService.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
}
