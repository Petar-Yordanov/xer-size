using System.Windows.Input;

namespace XerSize.Components;

public partial class AppDialog : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(AppDialog), false);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AppDialog), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(AppDialog),
            string.Empty,
            propertyChanged: OnMessageChanged);

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(AppDialog),
            default(ImageSource),
            propertyChanged: OnIconChanged);

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(AppDialog), Colors.Transparent);

    public static readonly BindableProperty IconStrokeColorProperty =
        BindableProperty.Create(nameof(IconStrokeColor), typeof(Color), typeof(AppDialog), Colors.Transparent);

    public static readonly BindableProperty IconStrokeThicknessProperty =
        BindableProperty.Create(nameof(IconStrokeThickness), typeof(double), typeof(AppDialog), 0d);

    public static readonly BindableProperty IconContainerSizeProperty =
        BindableProperty.Create(nameof(IconContainerSize), typeof(double), typeof(AppDialog), 48d);

    public static readonly BindableProperty IconSizeProperty =
        BindableProperty.Create(nameof(IconSize), typeof(double), typeof(AppDialog), 20d);

    public static readonly BindableProperty IconPaddingProperty =
        BindableProperty.Create(nameof(IconPadding), typeof(Thickness), typeof(AppDialog), new Thickness(12));

    public static readonly BindableProperty DialogContentProperty =
        BindableProperty.Create(
            nameof(DialogContent),
            typeof(View),
            typeof(AppDialog),
            null,
            propertyChanged: OnDialogContentChanged);

    public static readonly BindableProperty DismissCommandProperty =
        BindableProperty.Create(nameof(DismissCommand), typeof(ICommand), typeof(AppDialog));

    public static readonly BindableProperty PrimaryCommandProperty =
        BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(AppDialog));

    public static readonly BindableProperty SecondaryCommandProperty =
        BindableProperty.Create(nameof(SecondaryCommand), typeof(ICommand), typeof(AppDialog));

    public static readonly BindableProperty PrimaryButtonTextProperty =
        BindableProperty.Create(nameof(PrimaryButtonText), typeof(string), typeof(AppDialog), "OK");

    public static readonly BindableProperty SecondaryButtonTextProperty =
        BindableProperty.Create(nameof(SecondaryButtonText), typeof(string), typeof(AppDialog), "Cancel");

    public static readonly BindableProperty ShowPrimaryButtonProperty =
        BindableProperty.Create(
            nameof(ShowPrimaryButton),
            typeof(bool),
            typeof(AppDialog),
            true,
            propertyChanged: OnButtonVisibilityChanged);

    public static readonly BindableProperty ShowSecondaryButtonProperty =
        BindableProperty.Create(
            nameof(ShowSecondaryButton),
            typeof(bool),
            typeof(AppDialog),
            true,
            propertyChanged: OnButtonVisibilityChanged);

    public static readonly BindableProperty PrimaryButtonBackgroundColorProperty =
        BindableProperty.Create(nameof(PrimaryButtonBackgroundColor), typeof(Color), typeof(AppDialog), Color.FromArgb("#16A34A"));

    public static readonly BindableProperty PrimaryButtonStrokeColorProperty =
        BindableProperty.Create(nameof(PrimaryButtonStrokeColor), typeof(Color), typeof(AppDialog), Colors.Transparent);

    public static readonly BindableProperty PrimaryButtonStrokeThicknessProperty =
        BindableProperty.Create(nameof(PrimaryButtonStrokeThickness), typeof(double), typeof(AppDialog), 0d);

    public static readonly BindableProperty PrimaryButtonTextColorProperty =
        BindableProperty.Create(nameof(PrimaryButtonTextColor), typeof(Color), typeof(AppDialog), Colors.White);

    public static readonly BindableProperty PrimaryButtonShadowOpacityProperty =
        BindableProperty.Create(nameof(PrimaryButtonShadowOpacity), typeof(float), typeof(AppDialog), 0.10f);

    public static readonly BindableProperty SecondaryButtonBackgroundColorProperty =
        BindableProperty.Create(nameof(SecondaryButtonBackgroundColor), typeof(Color), typeof(AppDialog), Color.FromArgb("#F0FBF4"));

    public static readonly BindableProperty SecondaryButtonStrokeColorProperty =
        BindableProperty.Create(nameof(SecondaryButtonStrokeColor), typeof(Color), typeof(AppDialog), Color.FromArgb("#C7E3CF"));

    public static readonly BindableProperty SecondaryButtonStrokeThicknessProperty =
        BindableProperty.Create(nameof(SecondaryButtonStrokeThickness), typeof(double), typeof(AppDialog), 1d);

    public static readonly BindableProperty SecondaryButtonTextColorProperty =
        BindableProperty.Create(nameof(SecondaryButtonTextColor), typeof(Color), typeof(AppDialog), Color.FromArgb("#0F1712"));

    public static readonly BindableProperty SecondaryButtonShadowOpacityProperty =
        BindableProperty.Create(nameof(SecondaryButtonShadowOpacity), typeof(float), typeof(AppDialog), 0f);

    public static readonly BindableProperty OverlayColorProperty =
        BindableProperty.Create(nameof(OverlayColor), typeof(Color), typeof(AppDialog), Color.FromArgb("#88000000"));

    public static readonly BindableProperty DialogMarginProperty =
        BindableProperty.Create(nameof(DialogMargin), typeof(Thickness), typeof(AppDialog), new Thickness(20));

    public static readonly BindableProperty DialogPaddingProperty =
        BindableProperty.Create(nameof(DialogPadding), typeof(Thickness), typeof(AppDialog), new Thickness(20));

    public static readonly BindableProperty MaximumDialogWidthProperty =
        BindableProperty.Create(nameof(MaximumDialogWidth), typeof(double), typeof(AppDialog), 420d);

    public static readonly BindableProperty DialogVerticalOptionsProperty =
        BindableProperty.Create(nameof(DialogVerticalOptions), typeof(LayoutOptions), typeof(AppDialog), LayoutOptions.Center);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(AppDialog), 28d);

    public static readonly BindableProperty ContentSpacingProperty =
        BindableProperty.Create(nameof(ContentSpacing), typeof(double), typeof(AppDialog), 16d);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public Color IconStrokeColor
    {
        get => (Color)GetValue(IconStrokeColorProperty);
        set => SetValue(IconStrokeColorProperty, value);
    }

    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }

    public double IconContainerSize
    {
        get => (double)GetValue(IconContainerSizeProperty);
        set => SetValue(IconContainerSizeProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Thickness IconPadding
    {
        get => (Thickness)GetValue(IconPaddingProperty);
        set => SetValue(IconPaddingProperty, value);
    }

    public View? DialogContent
    {
        get => (View?)GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    public ICommand? DismissCommand
    {
        get => (ICommand?)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public ICommand? PrimaryCommand
    {
        get => (ICommand?)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public ICommand? SecondaryCommand
    {
        get => (ICommand?)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public string SecondaryButtonText
    {
        get => (string)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public bool ShowPrimaryButton
    {
        get => (bool)GetValue(ShowPrimaryButtonProperty);
        set => SetValue(ShowPrimaryButtonProperty, value);
    }

    public bool ShowSecondaryButton
    {
        get => (bool)GetValue(ShowSecondaryButtonProperty);
        set => SetValue(ShowSecondaryButtonProperty, value);
    }

    public Color PrimaryButtonBackgroundColor
    {
        get => (Color)GetValue(PrimaryButtonBackgroundColorProperty);
        set => SetValue(PrimaryButtonBackgroundColorProperty, value);
    }

    public Color PrimaryButtonStrokeColor
    {
        get => (Color)GetValue(PrimaryButtonStrokeColorProperty);
        set => SetValue(PrimaryButtonStrokeColorProperty, value);
    }

    public double PrimaryButtonStrokeThickness
    {
        get => (double)GetValue(PrimaryButtonStrokeThicknessProperty);
        set => SetValue(PrimaryButtonStrokeThicknessProperty, value);
    }

    public Color PrimaryButtonTextColor
    {
        get => (Color)GetValue(PrimaryButtonTextColorProperty);
        set => SetValue(PrimaryButtonTextColorProperty, value);
    }

    public float PrimaryButtonShadowOpacity
    {
        get => (float)GetValue(PrimaryButtonShadowOpacityProperty);
        set => SetValue(PrimaryButtonShadowOpacityProperty, value);
    }

    public Color SecondaryButtonBackgroundColor
    {
        get => (Color)GetValue(SecondaryButtonBackgroundColorProperty);
        set => SetValue(SecondaryButtonBackgroundColorProperty, value);
    }

    public Color SecondaryButtonStrokeColor
    {
        get => (Color)GetValue(SecondaryButtonStrokeColorProperty);
        set => SetValue(SecondaryButtonStrokeColorProperty, value);
    }

    public double SecondaryButtonStrokeThickness
    {
        get => (double)GetValue(SecondaryButtonStrokeThicknessProperty);
        set => SetValue(SecondaryButtonStrokeThicknessProperty, value);
    }

    public Color SecondaryButtonTextColor
    {
        get => (Color)GetValue(SecondaryButtonTextColorProperty);
        set => SetValue(SecondaryButtonTextColorProperty, value);
    }

    public float SecondaryButtonShadowOpacity
    {
        get => (float)GetValue(SecondaryButtonShadowOpacityProperty);
        set => SetValue(SecondaryButtonShadowOpacityProperty, value);
    }

    public Color OverlayColor
    {
        get => (Color)GetValue(OverlayColorProperty);
        set => SetValue(OverlayColorProperty, value);
    }

    public Thickness DialogMargin
    {
        get => (Thickness)GetValue(DialogMarginProperty);
        set => SetValue(DialogMarginProperty, value);
    }

    public Thickness DialogPadding
    {
        get => (Thickness)GetValue(DialogPaddingProperty);
        set => SetValue(DialogPaddingProperty, value);
    }

    public double MaximumDialogWidth
    {
        get => (double)GetValue(MaximumDialogWidthProperty);
        set => SetValue(MaximumDialogWidthProperty, value);
    }

    public LayoutOptions DialogVerticalOptions
    {
        get => (LayoutOptions)GetValue(DialogVerticalOptionsProperty);
        set => SetValue(DialogVerticalOptionsProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double ContentSpacing
    {
        get => (double)GetValue(ContentSpacingProperty);
        set => SetValue(ContentSpacingProperty, value);
    }

    public bool ShowIcon => IconSource is not null;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool HasDialogContent => DialogContent is not null;

    public bool ShowButtons => ShowPrimaryButton || ShowSecondaryButton;

    public int PrimaryButtonColumn => ShowSecondaryButton ? 1 : 0;

    public int PrimaryButtonColumnSpan => ShowSecondaryButton ? 1 : 2;

    public AppDialog()
    {
        InitializeComponent();
        SyncDialogContent();
    }

    private void SyncDialogContent()
    {
        if (DialogContentHost is null)
            return;

        DialogContentHost.Content = DialogContent;
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDialog)bindable).OnPropertyChanged(nameof(ShowIcon));
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDialog)bindable).OnPropertyChanged(nameof(HasMessage));
    }

    private static void OnDialogContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var dialog = (AppDialog)bindable;

        dialog.SyncDialogContent();
        dialog.OnPropertyChanged(nameof(HasDialogContent));
    }

    private static void OnButtonVisibilityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var dialog = (AppDialog)bindable;

        dialog.OnPropertyChanged(nameof(ShowButtons));
        dialog.OnPropertyChanged(nameof(PrimaryButtonColumn));
        dialog.OnPropertyChanged(nameof(PrimaryButtonColumnSpan));
    }
}