using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace XerSize.Components;

public partial class AppRadarChart : ContentView
{
    public static readonly BindableProperty SeriesProperty =
        BindableProperty.Create(
            nameof(Series),
            typeof(IEnumerable<ISeries>),
            typeof(AppRadarChart),
            default(IEnumerable<ISeries>));

    public static readonly BindableProperty AngleAxesProperty =
        BindableProperty.Create(
            nameof(AngleAxes),
            typeof(IEnumerable<IPolarAxis>),
            typeof(AppRadarChart),
            default(IEnumerable<IPolarAxis>));

    public static readonly BindableProperty RadiusAxesProperty =
        BindableProperty.Create(
            nameof(RadiusAxes),
            typeof(IEnumerable<IPolarAxis>),
            typeof(AppRadarChart),
            default(IEnumerable<IPolarAxis>));

    public static readonly BindableProperty ChartHeightProperty =
        BindableProperty.Create(
            nameof(ChartHeight),
            typeof(double),
            typeof(AppRadarChart),
            280d);

    public static readonly BindableProperty ChartBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ChartBackgroundColor),
            typeof(Color),
            typeof(AppRadarChart),
            Colors.Transparent);

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

    public double ChartHeight
    {
        get => (double)GetValue(ChartHeightProperty);
        set => SetValue(ChartHeightProperty, value);
    }

    public Color ChartBackgroundColor
    {
        get => (Color)GetValue(ChartBackgroundColorProperty);
        set => SetValue(ChartBackgroundColorProperty, value);
    }

    public AppRadarChart()
    {
        InitializeComponent();
    }
}