using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class QuickActionLauncherButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(QuickActionLauncherButton), "Open Actions");

    public static readonly BindableProperty LeadingIconTextProperty =
        BindableProperty.Create(nameof(LeadingIconText), typeof(string), typeof(QuickActionLauncherButton), "⚡");

    public static readonly BindableProperty TrailingIconTextProperty =
        BindableProperty.Create(nameof(TrailingIconText), typeof(string), typeof(QuickActionLauncherButton), "▴");

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(QuickActionLauncherButton));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(QuickActionLauncherButton));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string LeadingIconText
    {
        get => (string)GetValue(LeadingIconTextProperty);
        set => SetValue(LeadingIconTextProperty, value);
    }

    public string TrailingIconText
    {
        get => (string)GetValue(TrailingIconTextProperty);
        set => SetValue(TrailingIconTextProperty, value);
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

    public QuickActionLauncherButton()
    {
        InitializeComponent();
    }
}