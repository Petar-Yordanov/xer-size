using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class RoutineDropdown : ContentView
{
    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(
            nameof(SelectedText),
            typeof(string),
            typeof(RoutineDropdown),
            string.Empty);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(RoutineDropdown));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(
            nameof(TapCommandParameter),
            typeof(object),
            typeof(RoutineDropdown));

    public static readonly BindableProperty AddCommandProperty =
        BindableProperty.Create(
            nameof(AddCommand),
            typeof(ICommand),
            typeof(RoutineDropdown));

    public static readonly BindableProperty AddCommandParameterProperty =
        BindableProperty.Create(
            nameof(AddCommandParameter),
            typeof(object),
            typeof(RoutineDropdown));

    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.Create(
            nameof(LongPressCommand),
            typeof(ICommand),
            typeof(RoutineDropdown));

    public static readonly BindableProperty LongPressCommandParameterProperty =
        BindableProperty.Create(
            nameof(LongPressCommandParameter),
            typeof(object),
            typeof(RoutineDropdown));

    public static readonly BindableProperty IsLongPressEnabledProperty =
        BindableProperty.Create(
            nameof(IsLongPressEnabled),
            typeof(bool),
            typeof(RoutineDropdown),
            true);

    public static readonly BindableProperty ShowAddButtonProperty =
        BindableProperty.Create(
            nameof(ShowAddButton),
            typeof(bool),
            typeof(RoutineDropdown),
            true);

    public static readonly BindableProperty AddButtonTextProperty =
        BindableProperty.Create(
            nameof(AddButtonText),
            typeof(string),
            typeof(RoutineDropdown),
            "+");

    public static readonly BindableProperty TrailingIconTextProperty =
        BindableProperty.Create(
            nameof(TrailingIconText),
            typeof(string),
            typeof(RoutineDropdown),
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

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public object? AddCommandParameter
    {
        get => GetValue(AddCommandParameterProperty);
        set => SetValue(AddCommandParameterProperty, value);
    }

    public ICommand? LongPressCommand
    {
        get => (ICommand?)GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    public object? LongPressCommandParameter
    {
        get => GetValue(LongPressCommandParameterProperty);
        set => SetValue(LongPressCommandParameterProperty, value);
    }

    public bool IsLongPressEnabled
    {
        get => (bool)GetValue(IsLongPressEnabledProperty);
        set => SetValue(IsLongPressEnabledProperty, value);
    }

    public bool ShowAddButton
    {
        get => (bool)GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    public string AddButtonText
    {
        get => (string)GetValue(AddButtonTextProperty);
        set => SetValue(AddButtonTextProperty, value);
    }

    public string TrailingIconText
    {
        get => (string)GetValue(TrailingIconTextProperty);
        set => SetValue(TrailingIconTextProperty, value);
    }

    public RoutineDropdown()
    {
        InitializeComponent();
    }
}