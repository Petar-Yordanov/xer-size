using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class Button : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Button), string.Empty);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Button));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Button));

    public static readonly BindableProperty ButtonStyleProperty =
        BindableProperty.Create(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(Button),
            defaultValue: null,
            defaultValueCreator: _ => Application.Current?.Resources["PrimaryButtonStyle"] as Style);

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

    public Style? ButtonStyle
    {
        get => (Style?)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public Button()
    {
        InitializeComponent();
    }
}