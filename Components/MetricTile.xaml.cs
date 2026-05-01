namespace XerSize.Components;

public partial class MetricTile : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(MetricTile), string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(MetricTile), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(MetricTile), string.Empty);

    public static readonly BindableProperty TileBackgroundColorProperty =
        BindableProperty.Create(nameof(TileBackgroundColor), typeof(Color), typeof(MetricTile), Colors.White);

    public static readonly BindableProperty ValueColorProperty =
        BindableProperty.Create(nameof(ValueColor), typeof(Color), typeof(MetricTile), Colors.Black);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public Color TileBackgroundColor
    {
        get => (Color)GetValue(TileBackgroundColorProperty);
        set => SetValue(TileBackgroundColorProperty, value);
    }

    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public MetricTile()
    {
        InitializeComponent();
    }
}