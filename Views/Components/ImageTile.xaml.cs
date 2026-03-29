using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class ImageTile : ContentView
{
    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(ImageTile),
            default(ImageSource),
            propertyChanged: OnImageStateChanged);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(ImageTile));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(
            nameof(TapCommandParameter),
            typeof(object),
            typeof(ImageTile));

    public static readonly BindableProperty PlaceholderTitleProperty =
        BindableProperty.Create(
            nameof(PlaceholderTitle),
            typeof(string),
            typeof(ImageTile),
            "Upload");

    public static readonly BindableProperty PlaceholderSubtitleProperty =
        BindableProperty.Create(
            nameof(PlaceholderSubtitle),
            typeof(string),
            typeof(ImageTile),
            "Tap anywhere");

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(
            nameof(ActionText),
            typeof(string),
            typeof(ImageTile),
            "Change");

    public static readonly BindableProperty TileHeightProperty =
        BindableProperty.Create(
            nameof(TileHeight),
            typeof(double),
            typeof(ImageTile),
            110d);

    public static readonly BindableProperty TileWidthProperty =
        BindableProperty.Create(
            nameof(TileWidth),
            typeof(double),
            typeof(ImageTile),
            -1d);

    public static readonly BindableProperty HasImageProperty =
        BindableProperty.Create(
            nameof(HasImage),
            typeof(bool),
            typeof(ImageTile),
            false);

    public static readonly BindableProperty ShowPlaceholderProperty =
        BindableProperty.Create(
            nameof(ShowPlaceholder),
            typeof(bool),
            typeof(ImageTile),
            true);

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public string PlaceholderTitle
    {
        get => (string)GetValue(PlaceholderTitleProperty);
        set => SetValue(PlaceholderTitleProperty, value);
    }

    public string PlaceholderSubtitle
    {
        get => (string)GetValue(PlaceholderSubtitleProperty);
        set => SetValue(PlaceholderSubtitleProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public double TileHeight
    {
        get => (double)GetValue(TileHeightProperty);
        set => SetValue(TileHeightProperty, value);
    }

    public double TileWidth
    {
        get => (double)GetValue(TileWidthProperty);
        set => SetValue(TileWidthProperty, value);
    }

    public bool HasImage
    {
        get => (bool)GetValue(HasImageProperty);
        private set => SetValue(HasImageProperty, value);
    }

    public bool ShowPlaceholder
    {
        get => (bool)GetValue(ShowPlaceholderProperty);
        private set => SetValue(ShowPlaceholderProperty, value);
    }

    private static void OnImageStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ImageTile)bindable;
        var hasImage = newValue is ImageSource;
        control.HasImage = hasImage;
        control.ShowPlaceholder = !hasImage;
    }

    public ImageTile()
    {
        InitializeComponent();
        var hasImage = ImageSource is not null;
        HasImage = hasImage;
        ShowPlaceholder = !hasImage;
    }
}