using LiveChartsCore;

namespace XerSize.Components;

public partial class AppPieChart : ContentView
{
    public static readonly BindableProperty SeriesProperty =
        BindableProperty.Create(
            nameof(Series),
            typeof(IEnumerable<ISeries>),
            typeof(AppPieChart),
            default(IEnumerable<ISeries>));

    public static readonly BindableProperty ChartHeightProperty =
        BindableProperty.Create(
            nameof(ChartHeight),
            typeof(double),
            typeof(AppPieChart),
            220d);

    public static readonly BindableProperty ChartBackgroundColorProperty =
        BindableProperty.Create(
            nameof(ChartBackgroundColor),
            typeof(Color),
            typeof(AppPieChart),
            Colors.Transparent);

    public IEnumerable<ISeries>? Series
    {
        get => (IEnumerable<ISeries>?)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
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

    public AppPieChart()
    {
        InitializeComponent();
    }
}