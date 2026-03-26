using System.Collections;
using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class RoutineDropdown : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(RoutineDropdown));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(RoutineDropdown), defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty ManageCommandProperty =
        BindableProperty.Create(nameof(ManageCommand), typeof(ICommand), typeof(RoutineDropdown));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? ManageCommand
    {
        get => (ICommand?)GetValue(ManageCommandProperty);
        set => SetValue(ManageCommandProperty, value);
    }

    public RoutineDropdown()
    {
        InitializeComponent();
    }
}
