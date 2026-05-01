using System.Windows.Input;

namespace XerSize.Components;

public partial class ToggleField : ContentView
{
    private const double ThumbTravel = 22d;

    private bool hasLoaded;

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(ToggleField), string.Empty);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ToggleField), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(ToggleField), string.Empty);

    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(
            nameof(IsToggled),
            typeof(bool),
            typeof(ToggleField),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(ToggleField));

    public static readonly BindableProperty FieldBackgroundColorProperty =
        BindableProperty.Create(nameof(FieldBackgroundColor), typeof(Color), typeof(ToggleField), Colors.White);

    public static readonly BindableProperty TrackOnColorProperty =
        BindableProperty.Create(
            nameof(TrackOnColor),
            typeof(Color),
            typeof(ToggleField),
            Colors.LightGray,
            propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty TrackOffColorProperty =
        BindableProperty.Create(
            nameof(TrackOffColor),
            typeof(Color),
            typeof(ToggleField),
            Colors.LightGray,
            propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty ThumbOnColorProperty =
        BindableProperty.Create(
            nameof(ThumbOnColor),
            typeof(Color),
            typeof(ToggleField),
            Colors.Orange,
            propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty ThumbOffColorProperty =
        BindableProperty.Create(
            nameof(ThumbOffColor),
            typeof(Color),
            typeof(ToggleField),
            Colors.White,
            propertyChanged: OnVisualStateChanged);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public Color FieldBackgroundColor
    {
        get => (Color)GetValue(FieldBackgroundColorProperty);
        set => SetValue(FieldBackgroundColorProperty, value);
    }

    public Color TrackOnColor
    {
        get => (Color)GetValue(TrackOnColorProperty);
        set => SetValue(TrackOnColorProperty, value);
    }

    public Color TrackOffColor
    {
        get => (Color)GetValue(TrackOffColorProperty);
        set => SetValue(TrackOffColorProperty, value);
    }

    public Color ThumbOnColor
    {
        get => (Color)GetValue(ThumbOnColorProperty);
        set => SetValue(ThumbOnColorProperty, value);
    }

    public Color ThumbOffColor
    {
        get => (Color)GetValue(ThumbOffColorProperty);
        set => SetValue(ThumbOffColorProperty, value);
    }

    public Color EffectiveTrackColor => IsToggled ? TrackOnColor : TrackOffColor;

    public Color EffectiveThumbColor => IsToggled ? ThumbOnColor : ThumbOffColor;

    public ToggleField()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        hasLoaded = true;

        ToggleThumb.TranslationX = IsToggled ? ThumbTravel : 0;
    }

    private static void OnVisualStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ToggleField)bindable;

        view.OnPropertyChanged(nameof(EffectiveTrackColor));
        view.OnPropertyChanged(nameof(EffectiveThumbColor));

        view.AnimateThumb();
    }

    private async void AnimateThumb()
    {
        if (!hasLoaded)
            return;

        var targetX = IsToggled ? ThumbTravel : 0;

        await ToggleThumb.TranslateToAsync(targetX, 0, 130, Easing.CubicOut);
    }
}