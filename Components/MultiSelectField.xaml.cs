using System.Collections;

namespace XerSize.Components;

public partial class MultiSelectField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(MultiSelectField), string.Empty);

    public static readonly BindableProperty SummaryTextProperty =
        BindableProperty.Create(nameof(SummaryText), typeof(string), typeof(MultiSelectField), string.Empty, propertyChanged: OnSummaryChanged);

    public static readonly BindableProperty HasSelectionProperty =
        BindableProperty.Create(nameof(HasSelection), typeof(bool), typeof(MultiSelectField), false);

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(MultiSelectField), false);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MultiSelectField), null);

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(MultiSelectField), null);

    public static readonly BindableProperty DropdownBackgroundColorProperty =
        BindableProperty.Create(nameof(DropdownBackgroundColor), typeof(Color), typeof(MultiSelectField), Colors.White);

    public static readonly BindableProperty PrimaryActionContentProperty =
        BindableProperty.Create(nameof(PrimaryActionContent), typeof(View), typeof(MultiSelectField), null);

    public static readonly BindableProperty SecondaryActionContentProperty =
        BindableProperty.Create(nameof(SecondaryActionContent), typeof(View), typeof(MultiSelectField), null);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string SummaryText
    {
        get => (string)GetValue(SummaryTextProperty);
        set => SetValue(SummaryTextProperty, value);
    }

    public bool HasSelection
    {
        get => (bool)GetValue(HasSelectionProperty);
        set => SetValue(HasSelectionProperty, value);
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

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public Color DropdownBackgroundColor
    {
        get => (Color)GetValue(DropdownBackgroundColorProperty);
        set => SetValue(DropdownBackgroundColorProperty, value);
    }

    public View? PrimaryActionContent
    {
        get => (View?)GetValue(PrimaryActionContentProperty);
        set => SetValue(PrimaryActionContentProperty, value);
    }

    public View? SecondaryActionContent
    {
        get => (View?)GetValue(SecondaryActionContentProperty);
        set => SetValue(SecondaryActionContentProperty, value);
    }

    public MultiSelectField()
    {
        InitializeComponent();
    }

    private static void OnSummaryChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MultiSelectField)bindable;
        view.HasSelection = !string.IsNullOrWhiteSpace(view.SummaryText) &&
                            !string.Equals(view.SummaryText, "Select items", StringComparison.OrdinalIgnoreCase);
    }
}