using System.Windows.Input;

namespace XerSize.Components;

public partial class QuickActionsMenu : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(QuickActionsMenu), false);

    public static readonly BindableProperty MenuMarginProperty =
        BindableProperty.Create(nameof(MenuMargin), typeof(Thickness), typeof(QuickActionsMenu), new Thickness(20, 0, 20, 126));

    public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(QuickActionsMenu));

    public static readonly BindableProperty PrimaryTitleProperty =
        BindableProperty.Create(nameof(PrimaryTitle), typeof(string), typeof(QuickActionsMenu), "Add workout");

    public static readonly BindableProperty PrimarySubtitleProperty =
        BindableProperty.Create(nameof(PrimarySubtitle), typeof(string), typeof(QuickActionsMenu), "Create a new workout day");

    public static readonly BindableProperty PrimaryIconSourceProperty =
        BindableProperty.Create(nameof(PrimaryIconSource), typeof(ImageSource), typeof(QuickActionsMenu), default(ImageSource));

    public static readonly BindableProperty PrimaryCommandProperty =
        BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(QuickActionsMenu));

    public static readonly BindableProperty SecondaryTitleProperty =
        BindableProperty.Create(nameof(SecondaryTitle), typeof(string), typeof(QuickActionsMenu), "Add exercise");

    public static readonly BindableProperty SecondarySubtitleProperty =
        BindableProperty.Create(nameof(SecondarySubtitle), typeof(string), typeof(QuickActionsMenu), "Add movement to this workout");

    public static readonly BindableProperty SecondaryIconSourceProperty =
        BindableProperty.Create(nameof(SecondaryIconSource), typeof(ImageSource), typeof(QuickActionsMenu), default(ImageSource));

    public static readonly BindableProperty SecondaryCommandProperty =
        BindableProperty.Create(nameof(SecondaryCommand), typeof(ICommand), typeof(QuickActionsMenu));

    public static readonly BindableProperty TertiaryTitleProperty =
        BindableProperty.Create(nameof(TertiaryTitle), typeof(string), typeof(QuickActionsMenu), "Start workout");

    public static readonly BindableProperty TertiarySubtitleProperty =
        BindableProperty.Create(nameof(TertiarySubtitle), typeof(string), typeof(QuickActionsMenu), "Begin logging this session");

    public static readonly BindableProperty TertiaryIconSourceProperty =
        BindableProperty.Create(nameof(TertiaryIconSource), typeof(ImageSource), typeof(QuickActionsMenu), default(ImageSource));

    public static readonly BindableProperty TertiaryCommandProperty =
        BindableProperty.Create(nameof(TertiaryCommand), typeof(ICommand), typeof(QuickActionsMenu));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public Thickness MenuMargin
    {
        get => (Thickness)GetValue(MenuMarginProperty);
        set => SetValue(MenuMarginProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public string PrimaryTitle
    {
        get => (string)GetValue(PrimaryTitleProperty);
        set => SetValue(PrimaryTitleProperty, value);
    }

    public string PrimarySubtitle
    {
        get => (string)GetValue(PrimarySubtitleProperty);
        set => SetValue(PrimarySubtitleProperty, value);
    }

    public ImageSource? PrimaryIconSource
    {
        get => (ImageSource?)GetValue(PrimaryIconSourceProperty);
        set => SetValue(PrimaryIconSourceProperty, value);
    }

    public ICommand? PrimaryCommand
    {
        get => (ICommand?)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public string SecondaryTitle
    {
        get => (string)GetValue(SecondaryTitleProperty);
        set => SetValue(SecondaryTitleProperty, value);
    }

    public string SecondarySubtitle
    {
        get => (string)GetValue(SecondarySubtitleProperty);
        set => SetValue(SecondarySubtitleProperty, value);
    }

    public ImageSource? SecondaryIconSource
    {
        get => (ImageSource?)GetValue(SecondaryIconSourceProperty);
        set => SetValue(SecondaryIconSourceProperty, value);
    }

    public ICommand? SecondaryCommand
    {
        get => (ICommand?)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public string TertiaryTitle
    {
        get => (string)GetValue(TertiaryTitleProperty);
        set => SetValue(TertiaryTitleProperty, value);
    }

    public string TertiarySubtitle
    {
        get => (string)GetValue(TertiarySubtitleProperty);
        set => SetValue(TertiarySubtitleProperty, value);
    }

    public ImageSource? TertiaryIconSource
    {
        get => (ImageSource?)GetValue(TertiaryIconSourceProperty);
        set => SetValue(TertiaryIconSourceProperty, value);
    }

    public ICommand? TertiaryCommand
    {
        get => (ICommand?)GetValue(TertiaryCommandProperty);
        set => SetValue(TertiaryCommandProperty, value);
    }

    public QuickActionsMenu()
    {
        InitializeComponent();
    }
}