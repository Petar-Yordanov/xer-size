using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace XerSize.Components;

public partial class RadarChartCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(RadarChartCard), string.Empty);

    public static readonly BindableProperty DotColorProperty =
        BindableProperty.Create(nameof(DotColor), typeof(Color), typeof(RadarChartCard), Colors.Transparent, propertyChanged: OnDotColorChanged);

    public static readonly BindableProperty CardBackgroundColorProperty =
        BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(RadarChartCard), Colors.White);

    public static readonly BindableProperty ChartBackgroundColorProperty =
        BindableProperty.Create(nameof(ChartBackgroundColor), typeof(Color), typeof(RadarChartCard), Colors.Transparent);

    public static readonly BindableProperty ChartHeightProperty =
        BindableProperty.Create(nameof(ChartHeight), typeof(double), typeof(RadarChartCard), 240d);

    public static readonly BindableProperty SeriesProperty =
        BindableProperty.Create(nameof(Series), typeof(IEnumerable<ISeries>), typeof(RadarChartCard), null);

    public static readonly BindableProperty AngleAxesProperty =
        BindableProperty.Create(nameof(AngleAxes), typeof(IEnumerable<IPolarAxis>), typeof(RadarChartCard), null);

    public static readonly BindableProperty RadiusAxesProperty =
        BindableProperty.Create(nameof(RadiusAxes), typeof(IEnumerable<IPolarAxis>), typeof(RadarChartCard), null);

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

    public Color ChartBackgroundColor
    {
        get => (Color)GetValue(ChartBackgroundColorProperty);
        set => SetValue(ChartBackgroundColorProperty, value);
    }

    public double ChartHeight
    {
        get => (double)GetValue(ChartHeightProperty);
        set => SetValue(ChartHeightProperty, value);
    }

    public IEnumerable<ISeries>? Series
    {
        get => (IEnumerable<ISeries>?)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public IEnumerable<IPolarAxis>? AngleAxes
    {
        get => (IEnumerable<IPolarAxis>?)GetValue(AngleAxesProperty);
        set => SetValue(AngleAxesProperty, value);
    }

    public IEnumerable<IPolarAxis>? RadiusAxes
    {
        get => (IEnumerable<IPolarAxis>?)GetValue(RadiusAxesProperty);
        set => SetValue(RadiusAxesProperty, value);
    }

    public bool ShowDot => DotColor != Colors.Transparent;

    public RadarChartCard()
    {
        InitializeComponent();
    }

    private static void OnDotColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RadarChartCard)bindable).OnPropertyChanged(nameof(ShowDot));
    }
}