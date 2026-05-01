namespace XerSize.Components;

public partial class TextField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(TextField), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TextField), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(TextField), string.Empty);

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(nameof(IconSource), typeof(ImageSource), typeof(TextField), default(ImageSource), propertyChanged: OnIconChanged);

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

    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public bool HasIcon => IconSource is not null;

    public int EntryColumn => HasIcon ? 1 : 0;

    public TextField()
    {
        InitializeComponent();
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (TextField)bindable;
        view.OnPropertyChanged(nameof(HasIcon));
        view.OnPropertyChanged(nameof(EntryColumn));
    }
}