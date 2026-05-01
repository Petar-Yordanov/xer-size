namespace XerSize.Components;

public partial class ChartCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ChartCard), string.Empty);

    public static readonly BindableProperty DotColorProperty =
        BindableProperty.Create(nameof(DotColor), typeof(Color), typeof(ChartCard), Colors.Transparent, propertyChanged: OnDotColorChanged);

    public static readonly BindableProperty CardBackgroundColorProperty =
        BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(ChartCard), Colors.White);

    public static readonly BindableProperty CardContentProperty =
        BindableProperty.Create(nameof(CardContent), typeof(View), typeof(ChartCard), null);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Color DotColor
    {
        get => (Color)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public View? CardContent
    {
        get => (View?)GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    public bool ShowDot => DotColor != Colors.Transparent;

    public ChartCard()
    {
        InitializeComponent();
    }

    private static void OnDotColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((ChartCard)bindable).OnPropertyChanged(nameof(ShowDot));
    }
}