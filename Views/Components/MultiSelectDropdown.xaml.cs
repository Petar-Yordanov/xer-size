using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class MultiSelectDropdown : ContentView
{
    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(
            nameof(SelectedText),
            typeof(string),
            typeof(MultiSelectDropdown),
            string.Empty);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(MultiSelectDropdown));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(
            nameof(TapCommandParameter),
            typeof(object),
            typeof(MultiSelectDropdown));

    public static readonly BindableProperty TrailingIconTextProperty =
        BindableProperty.Create(
            nameof(TrailingIconText),
            typeof(string),
            typeof(MultiSelectDropdown),
            "v");

    public string SelectedText
    {
        get => (string)GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public string TrailingIconText
    {
        get => (string)GetValue(TrailingIconTextProperty);
        set => SetValue(TrailingIconTextProperty, value);
    }

    public MultiSelectDropdown()
    {
        InitializeComponent();
    }
}