using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class TabChip : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TabChip), string.Empty);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TabChip));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TabChip));

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(TabChip),
            false,
            propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty ChipPaddingProperty =
        BindableProperty.Create(
            nameof(ChipPadding),
            typeof(Thickness),
            typeof(TabChip),
            new Thickness(14, 9));

    public static readonly BindableProperty ChipHeightProperty =
        BindableProperty.Create(
            nameof(ChipHeight),
            typeof(double),
            typeof(TabChip),
            36d);

    public static readonly BindableProperty ChipMinWidthProperty =
        BindableProperty.Create(
            nameof(ChipMinWidth),
            typeof(double),
            typeof(TabChip),
            92d);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Thickness ChipPadding
    {
        get => (Thickness)GetValue(ChipPaddingProperty);
        set => SetValue(ChipPaddingProperty, value);
    }

    public double ChipHeight
    {
        get => (double)GetValue(ChipHeightProperty);
        set => SetValue(ChipHeightProperty, value);
    }

    public double ChipMinWidth
    {
        get => (double)GetValue(ChipMinWidthProperty);
        set => SetValue(ChipMinWidthProperty, value);
    }

    private static void OnVisualStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not TabChip chip)
            return;

        if ((bool)newValue)
        {
            chip.BackgroundColor = Colors.Transparent;
        }
        else
        {
            chip.BackgroundColor = Colors.Transparent;
        }
    }

    public TabChip()
    {
        InitializeComponent();
    }
}