using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class Button : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(Button),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Button),
            null,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(Button),
            null,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ButtonStyleProperty =
        BindableProperty.Create(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(Button),
            null,
            propertyChanged: OnVisualPropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public Style? ButtonStyle
    {
        get => (Style?)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public Button()
    {
        InitializeComponent();
        ApplyVisualState();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        ApplyVisualState();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        ApplyVisualState();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsEnabled))
            ApplyVisualState();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Button button)
            button.ApplyVisualState();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled)
            return;

        if (Command?.CanExecute(CommandParameter) != true)
            return;

        try
        {
            await ButtonSurface.ScaleTo(0.985, 45);
            await ButtonSurface.ScaleTo(1.0, 45);
        }
        catch
        {
            // Ignore animation interruptions.
        }

        Command.Execute(CommandParameter);
    }

    private void ApplyVisualState()
    {
        if (ButtonSurface is null || ButtonLabel is null || Application.Current?.Resources is not ResourceDictionary resources)
            return;

        ButtonLabel.Text = Text ?? string.Empty;

        var primaryStyle = resources["PrimaryButtonStyle"] as Style;
        var secondaryStyle = resources["SecondaryButtonStyle"] as Style;
        var dangerStyle = resources["DangerButtonStyle"] as Style;
        var iconStyle = resources["IconButtonStyle"] as Style;

        var isSecondary = ReferenceEquals(ButtonStyle, secondaryStyle);
        var isDanger = ReferenceEquals(ButtonStyle, dangerStyle);
        var isIcon = ReferenceEquals(ButtonStyle, iconStyle);

        var primaryColor = GetColor(resources, "PrimaryColor", Colors.Blue);
        var onPrimaryColor = GetColor(resources, "OnPrimaryColor", Colors.White);
        var surfaceMutedColor = GetColor(resources, "SurfaceMutedColor", Colors.LightGray);
        var primaryTextColor = GetColor(resources, "PrimaryTextColor", Colors.Black);
        var borderColor = GetColor(resources, "BorderColor", Colors.Gray);
        var dangerColor = GetColor(resources, "DangerColor", Colors.Red);
        var disabledBackgroundColor = GetColor(resources, "DisabledBackgroundColor", Colors.LightGray);
        var disabledTextColor = GetColor(resources, "DisabledTextColor", Colors.DarkGray);

        if (!IsEnabled)
        {
            ButtonSurface.BackgroundColor = disabledBackgroundColor;
            ButtonSurface.Stroke = borderColor;
            ButtonLabel.TextColor = disabledTextColor;
            ButtonSurface.Opacity = 0.85;
            return;
        }

        ButtonSurface.Opacity = 1.0;

        if (isDanger)
        {
            ButtonSurface.BackgroundColor = surfaceMutedColor;
            ButtonSurface.Stroke = dangerColor;
            ButtonLabel.TextColor = dangerColor;
            ButtonSurface.Padding = new Thickness(12, 9);
            ButtonSurface.MinimumHeightRequest = 40;
            ButtonSurface.WidthRequest = -1;
            ButtonSurface.HeightRequest = -1;
            ButtonShape.CornerRadius = 14;
            return;
        }

        if (isSecondary)
        {
            ButtonSurface.BackgroundColor = surfaceMutedColor;
            ButtonSurface.Stroke = borderColor;
            ButtonLabel.TextColor = primaryTextColor;
            ButtonSurface.Padding = new Thickness(12, 9);
            ButtonSurface.MinimumHeightRequest = 40;
            ButtonSurface.WidthRequest = -1;
            ButtonSurface.HeightRequest = -1;
            ButtonShape.CornerRadius = 14;
            return;
        }

        if (isIcon)
        {
            ButtonSurface.BackgroundColor = surfaceMutedColor;
            ButtonSurface.Stroke = borderColor;
            ButtonLabel.TextColor = primaryTextColor;
            ButtonSurface.Padding = new Thickness(0);
            ButtonSurface.MinimumHeightRequest = 42;
            ButtonSurface.WidthRequest = 42;
            ButtonSurface.HeightRequest = 42;
            ButtonShape.CornerRadius = 14;
            return;
        }

        ButtonSurface.BackgroundColor = primaryColor;
        ButtonSurface.Stroke = primaryColor;
        ButtonLabel.TextColor = onPrimaryColor;
        ButtonSurface.Padding = new Thickness(14, 10);
        ButtonSurface.MinimumHeightRequest = 42;
        ButtonSurface.WidthRequest = -1;
        ButtonSurface.HeightRequest = -1;
        ButtonShape.CornerRadius = 14;
    }

    private static Color GetColor(ResourceDictionary resources, string key, Color fallback)
    {
        return resources.TryGetValue(key, out var value) && value is Color color
            ? color
            : fallback;
    }
}