using System.Collections;
using System.Windows.Input;

namespace XerSize.Components;

public partial class SingleSelectField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(SingleSelectField), string.Empty);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SingleSelectField), string.Empty);

    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(nameof(SelectedText), typeof(string), typeof(SingleSelectField), string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(SingleSelectField), false);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SingleSelectField), null);

    public static readonly BindableProperty SelectItemCommandProperty =
        BindableProperty.Create(nameof(SelectItemCommand), typeof(ICommand), typeof(SingleSelectField));

    public static readonly BindableProperty DropdownBackgroundColorProperty =
        BindableProperty.Create(nameof(DropdownBackgroundColor), typeof(Color), typeof(SingleSelectField), Colors.White);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string SelectedText
    {
        get => (string)GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand? SelectItemCommand
    {
        get => (ICommand?)GetValue(SelectItemCommandProperty);
        set => SetValue(SelectItemCommandProperty, value);
    }

    public Color DropdownBackgroundColor
    {
        get => (Color)GetValue(DropdownBackgroundColorProperty);
        set => SetValue(DropdownBackgroundColorProperty, value);
    }

    public string DisplayText => string.IsNullOrWhiteSpace(SelectedText) ? Placeholder : SelectedText;

    public bool HasSelection => !string.IsNullOrWhiteSpace(SelectedText);

    public SingleSelectField()
    {
        InitializeComponent();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SingleSelectField)bindable;
        view.OnPropertyChanged(nameof(DisplayText));
        view.OnPropertyChanged(nameof(HasSelection));
    }
}