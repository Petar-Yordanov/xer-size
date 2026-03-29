using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class SingleSelectDropdown : ContentView
{
    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(
            nameof(SelectedText),
            typeof(string),
            typeof(SingleSelectDropdown),
            string.Empty);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(SingleSelectDropdown));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(
            nameof(TapCommandParameter),
            typeof(object),
            typeof(SingleSelectDropdown));

    public static readonly BindableProperty TrailingIconTextProperty =
        BindableProperty.Create(
            nameof(TrailingIconText),
            typeof(string),
            typeof(SingleSelectDropdown),
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

    public SingleSelectDropdown()
    {
        InitializeComponent();
    }
}