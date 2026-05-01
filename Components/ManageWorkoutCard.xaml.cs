using System.Windows.Input;

namespace XerSize.Components;

public partial class ManageWorkoutCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ManageWorkoutCard), string.Empty);

    public static readonly BindableProperty OrderTextProperty =
        BindableProperty.Create(nameof(OrderText), typeof(string), typeof(ManageWorkoutCard), string.Empty);

    public static readonly BindableProperty SettingsTextProperty =
        BindableProperty.Create(nameof(SettingsText), typeof(string), typeof(ManageWorkoutCard), string.Empty);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ManageWorkoutCard));

    public static readonly BindableProperty SettingsCommandProperty =
        BindableProperty.Create(nameof(SettingsCommand), typeof(ICommand), typeof(ManageWorkoutCard));

    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ManageWorkoutCard));

    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(ManageWorkoutCard));

    public static readonly BindableProperty ReorderCommandProperty =
        BindableProperty.Create(nameof(ReorderCommand), typeof(ICommand), typeof(ManageWorkoutCard));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string OrderText
    {
        get => (string)GetValue(OrderTextProperty);
        set => SetValue(OrderTextProperty, value);
    }

    public string SettingsText
    {
        get => (string)GetValue(SettingsTextProperty);
        set => SetValue(SettingsTextProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ICommand? SettingsCommand
    {
        get => (ICommand?)GetValue(SettingsCommandProperty);
        set => SetValue(SettingsCommandProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public ICommand? ReorderCommand
    {
        get => (ICommand?)GetValue(ReorderCommandProperty);
        set => SetValue(ReorderCommandProperty, value);
    }

    public ManageWorkoutCard()
    {
        InitializeComponent();
    }
}