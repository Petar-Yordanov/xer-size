namespace XerSize.Components;

public partial class TextAreaField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(TextAreaField), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TextAreaField), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(TextAreaField), string.Empty);

    public static readonly BindableProperty MinHeightProperty =
        BindableProperty.Create(nameof(MinHeight), typeof(double), typeof(TextAreaField), 110d);

    public static readonly BindableProperty FieldBackgroundColorProperty =
        BindableProperty.Create(nameof(FieldBackgroundColor), typeof(Color), typeof(TextAreaField), Colors.White);

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

    public double MinHeight
    {
        get => (double)GetValue(MinHeightProperty);
        set => SetValue(MinHeightProperty, value);
    }

    public Color FieldBackgroundColor
    {
        get => (Color)GetValue(FieldBackgroundColorProperty);
        set => SetValue(FieldBackgroundColorProperty, value);
    }

    public TextAreaField()
    {
        InitializeComponent();
    }
}