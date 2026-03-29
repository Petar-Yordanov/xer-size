namespace XerSize.Views.Components;

public partial class BottomNavBarItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(BottomNavBarItem), string.Empty);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(BottomNavBarItem), default(ImageSource));

    public static readonly BindableProperty RouteProperty =
        BindableProperty.Create(nameof(Route), typeof(string), typeof(BottomNavBarItem), string.Empty);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(BottomNavBarItem),
            false,
            propertyChanged: OnSelectedChanged);

    public static readonly BindableProperty TextColorValueProperty =
        BindableProperty.Create(nameof(TextColorValue), typeof(Color), typeof(BottomNavBarItem), Colors.Gray);

    public static readonly BindableProperty IconOpacityProperty =
        BindableProperty.Create(nameof(IconOpacity), typeof(double), typeof(BottomNavBarItem), 0.72d);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Route
    {
        get => (string)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Color TextColorValue
    {
        get => (Color)GetValue(TextColorValueProperty);
        set => SetValue(TextColorValueProperty, value);
    }

    public double IconOpacity
    {
        get => (double)GetValue(IconOpacityProperty);
        set => SetValue(IconOpacityProperty, value);
    }

    public BottomNavBarItem()
    {
        InitializeComponent();
        ApplyState();
    }

    private static void OnSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBarItem item)
            item.ApplyState();
    }

    private void ApplyState()
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
            return;

        var primaryText = (Color)resources["PrimaryTextColor"];
        var secondaryText = (Color)resources["SecondaryTextColor"];
        var surface = (Color)resources["SurfaceColor"];

        SelectionSurface.BackgroundColor = IsSelected ? surface : Colors.Transparent;
        TextColorValue = IsSelected ? primaryText : secondaryText;
        IconOpacity = IsSelected ? 1.0 : 0.72;
    }

    private async void OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Route))
            return;

        await Shell.Current.GoToAsync($"//{Route}");
    }
}