namespace XerSize.Views.Components;

public partial class MetricTile : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(MetricTile),
            string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            nameof(Value),
            typeof(string),
            typeof(MetricTile),
            string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(MetricTile),
            string.Empty,
            propertyChanged: OnSubtitleChanged);

    public static readonly BindableProperty HasSubtitleProperty =
        BindableProperty.Create(
            nameof(HasSubtitle),
            typeof(bool),
            typeof(MetricTile),
            false);

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

    public bool HasSubtitle
    {
        get => (bool)GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    private static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MetricTile)bindable;
        control.HasSubtitle = !string.IsNullOrWhiteSpace(newValue as string);
    }

    public MetricTile()
    {
        InitializeComponent();
        HasSubtitle = !string.IsNullOrWhiteSpace(Subtitle);
    }
}