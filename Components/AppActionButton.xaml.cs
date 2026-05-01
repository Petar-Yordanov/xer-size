using System.Windows.Input;

namespace XerSize.Components;

public partial class AppActionButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AppActionButton), string.Empty);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AppActionButton));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AppActionButton));

    public static readonly BindableProperty ButtonBackgroundColorProperty =
        BindableProperty.Create(nameof(ButtonBackgroundColor), typeof(Color), typeof(AppActionButton), Colors.White);

    public static readonly BindableProperty ButtonStrokeColorProperty =
        BindableProperty.Create(nameof(ButtonStrokeColor), typeof(Color), typeof(AppActionButton), Colors.Transparent);

    public static readonly BindableProperty ButtonStrokeThicknessProperty =
        BindableProperty.Create(nameof(ButtonStrokeThickness), typeof(double), typeof(AppActionButton), 0d);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(AppActionButton), Colors.Black);

    public static readonly BindableProperty ShadowOpacityProperty =
        BindableProperty.Create(nameof(ShadowOpacity), typeof(float), typeof(AppActionButton), 0f);

    public static readonly BindableProperty PressedScaleProperty =
        BindableProperty.Create(nameof(PressedScale), typeof(double), typeof(AppActionButton), 0.96d);

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

    public Color ButtonBackgroundColor
    {
        get => (Color)GetValue(ButtonBackgroundColorProperty);
        set => SetValue(ButtonBackgroundColorProperty, value);
    }

    public Color ButtonStrokeColor
    {
        get => (Color)GetValue(ButtonStrokeColorProperty);
        set => SetValue(ButtonStrokeColorProperty, value);
    }

    public double ButtonStrokeThickness
    {
        get => (double)GetValue(ButtonStrokeThicknessProperty);
        set => SetValue(ButtonStrokeThicknessProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public float ShadowOpacity
    {
        get => (float)GetValue(ShadowOpacityProperty);
        set => SetValue(ShadowOpacityProperty, value);
    }

    public double PressedScale
    {
        get => (double)GetValue(PressedScaleProperty);
        set => SetValue(PressedScaleProperty, value);
    }

    public AppActionButton()
    {
        InitializeComponent();
    }
}