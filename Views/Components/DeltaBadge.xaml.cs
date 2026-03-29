namespace XerSize.Views.Components;

public partial class DeltaBadge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(DeltaBadge),
            string.Empty);

    public static readonly BindableProperty IsPositiveProperty =
        BindableProperty.Create(
            nameof(IsPositive),
            typeof(bool),
            typeof(DeltaBadge),
            false);

    public static readonly BindableProperty IsNegativeProperty =
        BindableProperty.Create(
            nameof(IsNegative),
            typeof(bool),
            typeof(DeltaBadge),
            false);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsPositive
    {
        get => (bool)GetValue(IsPositiveProperty);
        set => SetValue(IsPositiveProperty, value);
    }

    public bool IsNegative
    {
        get => (bool)GetValue(IsNegativeProperty);
        set => SetValue(IsNegativeProperty, value);
    }

    public DeltaBadge()
    {
        InitializeComponent();
    }
}