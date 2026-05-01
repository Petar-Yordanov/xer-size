using System.Windows.Input;

namespace XerSize.Components;

public partial class MultiSelectOptionRow : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(MultiSelectOptionRow), string.Empty);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(MultiSelectOptionRow), false);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MultiSelectOptionRow));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MultiSelectOptionRow));

    public static readonly BindableProperty SelectedBackgroundColorProperty =
        BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.White);

    public static readonly BindableProperty SelectedStrokeColorProperty =
        BindableProperty.Create(nameof(SelectedStrokeColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.Transparent);

    public static readonly BindableProperty UnselectedBackgroundColorProperty =
        BindableProperty.Create(nameof(UnselectedBackgroundColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.White);

    public static readonly BindableProperty UnselectedStrokeColorProperty =
        BindableProperty.Create(nameof(UnselectedStrokeColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.Transparent);

    public static readonly BindableProperty SelectedCheckBackgroundColorProperty =
        BindableProperty.Create(nameof(SelectedCheckBackgroundColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.Orange);

    public static readonly BindableProperty UnselectedCheckBackgroundColorProperty =
        BindableProperty.Create(nameof(UnselectedCheckBackgroundColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.White);

    public static readonly BindableProperty UnselectedCheckStrokeColorProperty =
        BindableProperty.Create(nameof(UnselectedCheckStrokeColor), typeof(Color), typeof(MultiSelectOptionRow), Colors.Gray);

    public static readonly BindableProperty CheckIconSourceProperty =
        BindableProperty.Create(nameof(CheckIconSource), typeof(ImageSource), typeof(MultiSelectOptionRow), default(ImageSource));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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

    public Color SelectedBackgroundColor
    {
        get => (Color)GetValue(SelectedBackgroundColorProperty);
        set => SetValue(SelectedBackgroundColorProperty, value);
    }

    public Color SelectedStrokeColor
    {
        get => (Color)GetValue(SelectedStrokeColorProperty);
        set => SetValue(SelectedStrokeColorProperty, value);
    }

    public Color UnselectedBackgroundColor
    {
        get => (Color)GetValue(UnselectedBackgroundColorProperty);
        set => SetValue(UnselectedBackgroundColorProperty, value);
    }

    public Color UnselectedStrokeColor
    {
        get => (Color)GetValue(UnselectedStrokeColorProperty);
        set => SetValue(UnselectedStrokeColorProperty, value);
    }

    public Color SelectedCheckBackgroundColor
    {
        get => (Color)GetValue(SelectedCheckBackgroundColorProperty);
        set => SetValue(SelectedCheckBackgroundColorProperty, value);
    }

    public Color UnselectedCheckBackgroundColor
    {
        get => (Color)GetValue(UnselectedCheckBackgroundColorProperty);
        set => SetValue(UnselectedCheckBackgroundColorProperty, value);
    }

    public Color UnselectedCheckStrokeColor
    {
        get => (Color)GetValue(UnselectedCheckStrokeColorProperty);
        set => SetValue(UnselectedCheckStrokeColorProperty, value);
    }

    public ImageSource? CheckIconSource
    {
        get => (ImageSource?)GetValue(CheckIconSourceProperty);
        set => SetValue(CheckIconSourceProperty, value);
    }

    public MultiSelectOptionRow()
    {
        InitializeComponent();
    }
}