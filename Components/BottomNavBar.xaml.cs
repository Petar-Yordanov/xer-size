using System.Windows.Input;
using XerSize.Models.Presentation.Navigation;

namespace XerSize.Components;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty LeftPrimaryItemProperty = BindableProperty.Create(nameof(LeftPrimaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBar));
    public static readonly BindableProperty LeftSecondaryItemProperty = BindableProperty.Create(nameof(LeftSecondaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBar));
    public static readonly BindableProperty RightPrimaryItemProperty = BindableProperty.Create(nameof(RightPrimaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBar));
    public static readonly BindableProperty RightSecondaryItemProperty = BindableProperty.Create(nameof(RightSecondaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBar));
    public static readonly BindableProperty ItemCommandProperty = BindableProperty.Create(nameof(ItemCommand), typeof(ICommand), typeof(BottomNavBar));
    public static readonly BindableProperty CenterCommandProperty = BindableProperty.Create(nameof(CenterCommand), typeof(ICommand), typeof(BottomNavBar));
    public static readonly BindableProperty CenterCommandParameterProperty = BindableProperty.Create(nameof(CenterCommandParameter), typeof(object), typeof(BottomNavBar));
    public static readonly BindableProperty CenterIconSourceProperty = BindableProperty.Create(nameof(CenterIconSource), typeof(ImageSource), typeof(BottomNavBar), default(ImageSource));
    public static readonly BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(Brush), typeof(BottomNavBar), Brush.Transparent);
    public static readonly BindableProperty BarStrokeColorProperty = BindableProperty.Create(nameof(BarStrokeColor), typeof(Brush), typeof(BottomNavBar), Brush.Transparent);
    public static readonly BindableProperty BarShadowBrushProperty = BindableProperty.Create(nameof(BarShadowBrush), typeof(Brush), typeof(BottomNavBar), Brush.Transparent);
    public static readonly BindableProperty CenterButtonBackgroundColorProperty = BindableProperty.Create(nameof(CenterButtonBackgroundColor), typeof(Color), typeof(BottomNavBar), Colors.Transparent);
    public static readonly BindableProperty CenterButtonSizeProperty = BindableProperty.Create(nameof(CenterButtonSize), typeof(double), typeof(BottomNavBar), 62d);
    public static readonly BindableProperty CenterIconSizeProperty = BindableProperty.Create(nameof(CenterIconSize), typeof(double), typeof(BottomNavBar), 20d);
    public static readonly BindableProperty ShowCenterButtonProperty = BindableProperty.Create(nameof(ShowCenterButton), typeof(bool), typeof(BottomNavBar), true);
    public static readonly BindableProperty CenterHaloPaddingProperty = BindableProperty.Create(nameof(CenterHaloPadding), typeof(double), typeof(BottomNavBar), 6d);
    public static readonly BindableProperty CenterHaloInnerInsetProperty = BindableProperty.Create(nameof(CenterHaloInnerInset), typeof(double), typeof(BottomNavBar), 1d);

    public BottomNavItemPresentationModel? LeftPrimaryItem { get => (BottomNavItemPresentationModel?)GetValue(LeftPrimaryItemProperty); set => SetValue(LeftPrimaryItemProperty, value); }

    public BottomNavItemPresentationModel? LeftSecondaryItem { get => (BottomNavItemPresentationModel?)GetValue(LeftSecondaryItemProperty); set => SetValue(LeftSecondaryItemProperty, value); }

    public BottomNavItemPresentationModel? RightPrimaryItem { get => (BottomNavItemPresentationModel?)GetValue(RightPrimaryItemProperty); set => SetValue(RightPrimaryItemProperty, value); }

    public BottomNavItemPresentationModel? RightSecondaryItem { get => (BottomNavItemPresentationModel?)GetValue(RightSecondaryItemProperty); set => SetValue(RightSecondaryItemProperty, value); }

    public ICommand? ItemCommand { get => (ICommand?)GetValue(ItemCommandProperty); set => SetValue(ItemCommandProperty, value); }

    public ICommand? CenterCommand { get => (ICommand?)GetValue(CenterCommandProperty); set => SetValue(CenterCommandProperty, value); }

    public object? CenterCommandParameter { get => GetValue(CenterCommandParameterProperty); set => SetValue(CenterCommandParameterProperty, value); }

    public ImageSource? CenterIconSource { get => (ImageSource?)GetValue(CenterIconSourceProperty); set => SetValue(CenterIconSourceProperty, value); }

    public Brush BarBackgroundColor { get => (Brush)GetValue(BarBackgroundColorProperty); set => SetValue(BarBackgroundColorProperty, value); }

    public Brush BarStrokeColor { get => (Brush)GetValue(BarStrokeColorProperty); set => SetValue(BarStrokeColorProperty, value); }

    public Brush BarShadowBrush { get => (Brush)GetValue(BarShadowBrushProperty); set => SetValue(BarShadowBrushProperty, value); }

    public Color CenterButtonBackgroundColor { get => (Color)GetValue(CenterButtonBackgroundColorProperty); set => SetValue(CenterButtonBackgroundColorProperty, value); }

    public double CenterButtonSize { get => (double)GetValue(CenterButtonSizeProperty); set => SetValue(CenterButtonSizeProperty, value); }

    public double CenterIconSize { get => (double)GetValue(CenterIconSizeProperty); set => SetValue(CenterIconSizeProperty, value); }

    public bool ShowCenterButton { get => (bool)GetValue(ShowCenterButtonProperty); set => SetValue(ShowCenterButtonProperty, value); }

    public double CenterHaloPadding { get => (double)GetValue(CenterHaloPaddingProperty); set => SetValue(CenterHaloPaddingProperty, value); }

    public double CenterHaloInnerInset { get => (double)GetValue(CenterHaloInnerInsetProperty); set => SetValue(CenterHaloInnerInsetProperty, value); }

    public double CenterHaloOuterSize => CenterButtonSize + (CenterHaloPadding * 2);

    public double CenterHaloInnerSize => Math.Max(0, CenterHaloOuterSize - (CenterHaloInnerInset * 2));

    public BottomNavBar()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName is nameof(CenterButtonSize) or nameof(CenterHaloPadding) or nameof(CenterHaloInnerInset))
        {
            base.OnPropertyChanged(nameof(CenterHaloOuterSize));
            base.OnPropertyChanged(nameof(CenterHaloInnerSize));
        }
    }
}