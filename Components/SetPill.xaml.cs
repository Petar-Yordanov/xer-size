namespace XerSize.Components;

public partial class SetPill : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(SetPill), string.Empty);

    public static readonly BindableProperty BackgroundColorValueProperty =
        BindableProperty.Create(nameof(BackgroundColorValue), typeof(Color), typeof(SetPill), Colors.White);

    public static readonly BindableProperty StrokeColorValueProperty =
        BindableProperty.Create(nameof(StrokeColorValue), typeof(Color), typeof(SetPill), Colors.Transparent);

    public static readonly BindableProperty TextColorValueProperty =
        BindableProperty.Create(nameof(TextColorValue), typeof(Color), typeof(SetPill), Colors.Black);

    public static readonly BindableProperty HorizontalContentAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalContentAlignment), typeof(TextAlignment), typeof(SetPill), TextAlignment.Start);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color BackgroundColorValue
    {
        get => (Color)GetValue(BackgroundColorValueProperty);
        set => SetValue(BackgroundColorValueProperty, value);
    }

    public Color StrokeColorValue
    {
        get => (Color)GetValue(StrokeColorValueProperty);
        set => SetValue(StrokeColorValueProperty, value);
    }

    public Color TextColorValue
    {
        get => (Color)GetValue(TextColorValueProperty);
        set => SetValue(TextColorValueProperty, value);
    }

    public TextAlignment HorizontalContentAlignment
    {
        get => (TextAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public SetPill()
    {
        InitializeComponent();
    }
}