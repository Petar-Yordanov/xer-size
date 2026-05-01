namespace XerSize.Components;

public partial class HeroCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(HeroCard), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(HeroCard), string.Empty);

    public static readonly BindableProperty CardBackgroundColorProperty =
        BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(HeroCard), Colors.Orange);

    public static readonly BindableProperty TitleColorProperty =
        BindableProperty.Create(nameof(TitleColor), typeof(Color), typeof(HeroCard), Colors.White);

    public static readonly BindableProperty SubtitleColorProperty =
        BindableProperty.Create(nameof(SubtitleColor), typeof(Color), typeof(HeroCard), Colors.White);

    public static readonly BindableProperty TopIconSourceProperty =
        BindableProperty.Create(nameof(TopIconSource), typeof(ImageSource), typeof(HeroCard), default(ImageSource), propertyChanged: OnTopIconChanged);

    public static readonly BindableProperty TopIconBackgroundColorProperty =
        BindableProperty.Create(nameof(TopIconBackgroundColor), typeof(Color), typeof(HeroCard), Colors.Transparent);

    public static readonly BindableProperty PrimaryActionContentProperty =
        BindableProperty.Create(nameof(PrimaryActionContent), typeof(View), typeof(HeroCard), null);

    public static readonly BindableProperty SecondaryActionContentProperty =
        BindableProperty.Create(nameof(SecondaryActionContent), typeof(View), typeof(HeroCard), null);

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

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public Color TitleColor
    {
        get => (Color)GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    public Color SubtitleColor
    {
        get => (Color)GetValue(SubtitleColorProperty);
        set => SetValue(SubtitleColorProperty, value);
    }

    public ImageSource? TopIconSource
    {
        get => (ImageSource?)GetValue(TopIconSourceProperty);
        set => SetValue(TopIconSourceProperty, value);
    }

    public Color TopIconBackgroundColor
    {
        get => (Color)GetValue(TopIconBackgroundColorProperty);
        set => SetValue(TopIconBackgroundColorProperty, value);
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

    public bool ShowTopIcon => TopIconSource is not null;

    public HeroCard()
    {
        InitializeComponent();
    }

    private static void OnTopIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((HeroCard)bindable).OnPropertyChanged(nameof(ShowTopIcon));
    }
}