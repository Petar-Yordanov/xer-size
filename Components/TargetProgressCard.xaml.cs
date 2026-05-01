namespace XerSize.Components;

public partial class TargetProgressCard : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(TargetProgressCard),
        string.Empty);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(string),
        typeof(TargetProgressCard),
        string.Empty);

    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle),
        typeof(string),
        typeof(TargetProgressCard),
        string.Empty);

    public static readonly BindableProperty ProgressTextProperty = BindableProperty.Create(
        nameof(ProgressText),
        typeof(string),
        typeof(TargetProgressCard),
        string.Empty);

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress),
        typeof(double),
        typeof(TargetProgressCard),
        0d,
        propertyChanged: OnProgressChanged);

    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(
        nameof(CardBackgroundColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.White);

    public static readonly BindableProperty ValueColorProperty = BindableProperty.Create(
        nameof(ValueColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.Black);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.White);

    public static readonly BindableProperty TrackStrokeColorProperty = BindableProperty.Create(
        nameof(TrackStrokeColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.Transparent);

    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
        nameof(FillColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.Green);

    public static readonly BindableProperty ProgressTextColorProperty = BindableProperty.Create(
        nameof(ProgressTextColor),
        typeof(Color),
        typeof(TargetProgressCard),
        Colors.Black);

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

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        set => SetValue(ProgressTextProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public Color TrackStrokeColor
    {
        get => (Color)GetValue(TrackStrokeColorProperty);
        set => SetValue(TrackStrokeColorProperty, value);
    }

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public Color ProgressTextColor
    {
        get => (Color)GetValue(ProgressTextColorProperty);
        set => SetValue(ProgressTextColorProperty, value);
    }

    public TargetProgressCard()
    {
        InitializeComponent();

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateProgressFillWidth();
    }

    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((TargetProgressCard)bindable).UpdateProgressFillWidth();
    }

    private void UpdateProgressFillWidth()
    {
        if (ProgressTrack.Width <= 0)
            return;

        var normalizedProgress = Math.Clamp(Progress, 0d, 1d);
        ProgressFill.WidthRequest = ProgressTrack.Width * normalizedProgress;
    }
}