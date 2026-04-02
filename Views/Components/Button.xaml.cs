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
            propertyChanged: OnAppearancePropertyChanged);

    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(
            nameof(ImageSource),
            typeof(string),
            typeof(Button),
            default(string),
            propertyChanged: OnAppearancePropertyChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Button));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(Button));

    public static readonly BindableProperty ButtonStyleProperty =
        BindableProperty.Create(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(Button),
            default(Style),
            propertyChanged: OnButtonStyleChanged);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(float),
            typeof(Button),
            14f,
            propertyChanged: OnAppearancePropertyChanged);

    public static readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(
            nameof(IsEnabled),
            typeof(bool),
            typeof(Button),
            true,
            propertyChanged: OnAppearancePropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? ImageSource
    {
        get => (string?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
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

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public Button()
    {
        InitializeComponent();
        ApplyButtonStyle();
        UpdateAppearance();
    }

    private static void OnAppearancePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Button button)
            button.UpdateAppearance();
    }

    private static void OnButtonStyleChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is Button button)
        {
            button.ApplyButtonStyle();
            button.UpdateAppearance();
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled)
            return;

        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }

    private void ApplyButtonStyle()
    {
        if (ButtonStyle is null)
            return;

        ButtonSurface.Style = ButtonStyle;
    }

    private void UpdateAppearance()
    {
        if (ButtonLabel is null || ButtonIcon is null || ButtonSurface is null || ButtonShape is null || ButtonContentGrid is null)
            return;

        ButtonLabel.Text = Text ?? string.Empty;

        var hasImage = !string.IsNullOrWhiteSpace(ImageSource);
        var hasText = !string.IsNullOrWhiteSpace(Text);

        ButtonIcon.IsVisible = hasImage;
        ButtonIcon.Source = hasImage ? ImageSource : null;

        if (hasImage && hasText)
        {
            ButtonContentGrid.ColumnDefinitions.Clear();
            ButtonContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ButtonContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Grid.SetColumn(ButtonIcon, 0);
            Grid.SetColumn(ButtonLabel, 1);

            ButtonContentGrid.ColumnSpacing = 8;
            ButtonLabel.HorizontalOptions = LayoutOptions.Center;
            ButtonLabel.HorizontalTextAlignment = TextAlignment.Center;
        }
        else if (hasImage)
        {
            ButtonContentGrid.ColumnDefinitions.Clear();
            ButtonContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Grid.SetColumn(ButtonIcon, 0);
            Grid.SetColumn(ButtonLabel, 0);

            ButtonContentGrid.ColumnSpacing = 0;
            ButtonLabel.Text = string.Empty;
        }
        else
        {
            ButtonContentGrid.ColumnDefinitions.Clear();
            ButtonContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Grid.SetColumn(ButtonLabel, 0);
            Grid.SetColumn(ButtonIcon, 0);

            ButtonContentGrid.ColumnSpacing = 0;
            ButtonLabel.HorizontalOptions = LayoutOptions.Center;
            ButtonLabel.HorizontalTextAlignment = TextAlignment.Center;
        }

        ButtonSurface.Opacity = IsEnabled ? 1.0 : 0.6;
        ButtonShape.CornerRadius = CornerRadius;
    }
}
