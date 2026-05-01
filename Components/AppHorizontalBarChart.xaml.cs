using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace XerSize.Components;

public partial class AppHorizontalBarChart : ContentView
{
    public static readonly BindableProperty SeriesProperty =
        BindableProperty.Create(
            nameof(Series),
            typeof(IEnumerable<ISeries>),
            typeof(AppHorizontalBarChart),
            default(IEnumerable<ISeries>));

    public static readonly BindableProperty XAxesProperty =
        BindableProperty.Create(
            nameof(XAxes),
            typeof(IEnumerable<ICartesianAxis>),
            typeof(AppHorizontalBarChart),
            default(IEnumerable<ICartesianAxis>));

    public static readonly BindableProperty YAxesProperty =
        BindableProperty.Create(
            nameof(YAxes),
            typeof(IEnumerable<ICartesianAxis>),
            typeof(AppHorizontalBarChart),
            default(IEnumerable<ICartesianAxis>));

    public static readonly BindableProperty ChartHeightProperty =
        BindableProperty.Create(
            nameof(ChartHeight),
            typeof(double),
            typeof(AppHorizontalBarChart),
            220d);

    public static readonly BindableProperty ChartBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ChartBackgroundColor),
            typeof(Color),
            typeof(AppHorizontalBarChart),
            Colors.Transparent);

    public IEnumerable<ISeries>? Series
    {
        get => (IEnumerable<ISeries>?)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public IEnumerable<ICartesianAxis>? XAxes
    {
        get => (IEnumerable<ICartesianAxis>?)GetValue(XAxesProperty);
        set => SetValue(XAxesProperty, value);
    }

    public IEnumerable<ICartesianAxis>? YAxes
    {
        get => (IEnumerable<ICartesianAxis>?)GetValue(YAxesProperty);
        set => SetValue(YAxesProperty, value);
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

    public AppHorizontalBarChart()
    {
        InitializeComponent();
    }
}