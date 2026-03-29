namespace XerSize.Views.Components;

public partial class TextArea : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(TextArea),
            string.Empty,
            propertyChanged: OnLabelChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(TextArea),
            string.Empty,
            BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(TextArea),
            string.Empty);

    public static readonly BindableProperty HasLabelProperty =
        BindableProperty.Create(
            nameof(HasLabel),
            typeof(bool),
            typeof(TextArea),
            false);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool HasLabel
    {
        get => (bool)GetValue(HasLabelProperty);
        private set => SetValue(HasLabelProperty, value);
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (TextArea)bindable;
        control.HasLabel = !string.IsNullOrWhiteSpace(newValue as string);
    }

    public TextArea()
    {
        InitializeComponent();
        HasLabel = !string.IsNullOrWhiteSpace(Label);
    }
}