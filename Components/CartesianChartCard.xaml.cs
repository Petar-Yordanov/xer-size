using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace XerSize.Components;

public partial class CartesianChartCard : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(CartesianChartCard), string.Empty);

    public static readonly BindableProperty DotColorProperty = BindableProperty.Create(nameof(DotColor), typeof(Color), typeof(CartesianChartCard), Colors.Transparent, propertyChanged: OnDotColorChanged);

    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(CartesianChartCard), Colors.White);

    public static readonly BindableProperty ChartBackgroundColorProperty = BindableProperty.Create(nameof(ChartBackgroundColor), typeof(Color), typeof(CartesianChartCard), Colors.Transparent);

    public static readonly BindableProperty ChartHeightProperty = BindableProperty.Create(nameof(ChartHeight), typeof(double), typeof(CartesianChartCard), 220d);

    public static readonly BindableProperty SeriesProperty = BindableProperty.Create(nameof(Series), typeof(IEnumerable<ISeries>), typeof(CartesianChartCard), null);

    public static readonly BindableProperty XAxesProperty = BindableProperty.Create(nameof(XAxes), typeof(IEnumerable<ICartesianAxis>), typeof(CartesianChartCard), null);

    public static readonly BindableProperty YAxesProperty = BindableProperty.Create(nameof(YAxes), typeof(IEnumerable<ICartesianAxis>), typeof(CartesianChartCard), null);

    public static readonly BindableProperty HasDataProperty = BindableProperty.Create(
        nameof(HasData),
        typeof(bool),
        typeof(CartesianChartCard),
        true,
        propertyChanged: OnHasDataChanged);

    public static readonly BindableProperty EmptyMessageProperty = BindableProperty.Create(
        nameof(EmptyMessage),
        typeof(string),
        typeof(CartesianChartCard),
        "There is no data available to show. Start and complete a workout to show graphs.");

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public Color DotColor { get => (Color)GetValue(DotColorProperty); set => SetValue(DotColorProperty, value); }

    public Color CardBackgroundColor { get => (Color)GetValue(CardBackgroundColorProperty); set => SetValue(CardBackgroundColorProperty, value); }

    public Color ChartBackgroundColor { get => (Color)GetValue(ChartBackgroundColorProperty); set => SetValue(ChartBackgroundColorProperty, value); }

    public double ChartHeight { get => (double)GetValue(ChartHeightProperty); set => SetValue(ChartHeightProperty, value); }

    public IEnumerable<ISeries>? Series { get => (IEnumerable<ISeries>?)GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }

    public IEnumerable<ICartesianAxis>? XAxes { get => (IEnumerable<ICartesianAxis>?)GetValue(XAxesProperty); set => SetValue(XAxesProperty, value); }

    public IEnumerable<ICartesianAxis>? YAxes { get => (IEnumerable<ICartesianAxis>?)GetValue(YAxesProperty); set => SetValue(YAxesProperty, value); }

    public bool HasData { get => (bool)GetValue(HasDataProperty); set => SetValue(HasDataProperty, value); }

    public string EmptyMessage { get => (string)GetValue(EmptyMessageProperty); set => SetValue(EmptyMessageProperty, value); }

    public bool ShowDot => DotColor != Colors.Transparent;

    public bool HasNoData => !HasData;

    public CartesianChartCard()
    {
        InitializeComponent();
    }

    private static void OnDotColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CartesianChartCard)bindable).OnPropertyChanged(nameof(ShowDot));
    }

    private static void OnHasDataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CartesianChartCard)bindable).OnPropertyChanged(nameof(HasNoData));
    }
}