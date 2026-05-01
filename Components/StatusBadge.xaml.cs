namespace XerSize.Components;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusBadge), string.Empty);

    public static readonly BindableProperty BadgeBackgroundColorProperty =
        BindableProperty.Create(nameof(BadgeBackgroundColor), typeof(Color), typeof(StatusBadge), Colors.Transparent);

    public static readonly BindableProperty BadgeTextColorProperty =
        BindableProperty.Create(nameof(BadgeTextColor), typeof(Color), typeof(StatusBadge), Colors.Black);

    public static readonly BindableProperty BadgeAccentColorProperty =
        BindableProperty.Create(nameof(BadgeAccentColor), typeof(Color), typeof(StatusBadge), Colors.Black);

    public static readonly BindableProperty BadgeStrokeColorProperty =
        BindableProperty.Create(nameof(BadgeStrokeColor), typeof(Color), typeof(StatusBadge), Colors.Transparent);

    public static readonly BindableProperty BadgeStrokeThicknessProperty =
        BindableProperty.Create(nameof(BadgeStrokeThickness), typeof(double), typeof(StatusBadge), 1d);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color BadgeBackgroundColor
    {
        get => (Color)GetValue(BadgeBackgroundColorProperty);
        set => SetValue(BadgeBackgroundColorProperty, value);
    }

    public Color BadgeTextColor
    {
        get => (Color)GetValue(BadgeTextColorProperty);
        set => SetValue(BadgeTextColorProperty, value);
    }

    public Color BadgeAccentColor
    {
        get => (Color)GetValue(BadgeAccentColorProperty);
        set => SetValue(BadgeAccentColorProperty, value);
    }

    public Color BadgeStrokeColor
    {
        get => (Color)GetValue(BadgeStrokeColorProperty);
        set => SetValue(BadgeStrokeColorProperty, value);
    }

    public double BadgeStrokeThickness
    {
        get => (double)GetValue(BadgeStrokeThicknessProperty);
        set => SetValue(BadgeStrokeThicknessProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
    }
}