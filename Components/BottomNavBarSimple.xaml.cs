using System.Windows.Input;
using XerSize.Models.Presentation.Navigation;

namespace XerSize.Components;

public partial class BottomNavBarSimple : ContentView
{
    public static readonly BindableProperty LeftPrimaryItemProperty = BindableProperty.Create(nameof(LeftPrimaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBarSimple));
    public static readonly BindableProperty LeftSecondaryItemProperty = BindableProperty.Create(nameof(LeftSecondaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBarSimple));
    public static readonly BindableProperty RightPrimaryItemProperty = BindableProperty.Create(nameof(RightPrimaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBarSimple));
    public static readonly BindableProperty RightSecondaryItemProperty = BindableProperty.Create(nameof(RightSecondaryItem), typeof(BottomNavItemPresentationModel), typeof(BottomNavBarSimple));
    public static readonly BindableProperty ItemCommandProperty = BindableProperty.Create(nameof(ItemCommand), typeof(ICommand), typeof(BottomNavBarSimple));
    public static readonly BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(Brush), typeof(BottomNavBarSimple), Brush.Transparent);
    public static readonly BindableProperty BarStrokeColorProperty = BindableProperty.Create(nameof(BarStrokeColor), typeof(Brush), typeof(BottomNavBarSimple), Brush.Transparent);
    public static readonly BindableProperty BarShadowBrushProperty = BindableProperty.Create(nameof(BarShadowBrush), typeof(Brush), typeof(BottomNavBarSimple), Brush.Transparent);

    public BottomNavItemPresentationModel? LeftPrimaryItem { get => (BottomNavItemPresentationModel?)GetValue(LeftPrimaryItemProperty); set => SetValue(LeftPrimaryItemProperty, value); }

    public BottomNavItemPresentationModel? LeftSecondaryItem { get => (BottomNavItemPresentationModel?)GetValue(LeftSecondaryItemProperty); set => SetValue(LeftSecondaryItemProperty, value); }

    public BottomNavItemPresentationModel? RightPrimaryItem { get => (BottomNavItemPresentationModel?)GetValue(RightPrimaryItemProperty); set => SetValue(RightPrimaryItemProperty, value); }

    public BottomNavItemPresentationModel? RightSecondaryItem { get => (BottomNavItemPresentationModel?)GetValue(RightSecondaryItemProperty); set => SetValue(RightSecondaryItemProperty, value); }

    public ICommand? ItemCommand { get => (ICommand?)GetValue(ItemCommandProperty); set => SetValue(ItemCommandProperty, value); }

    public Brush BarBackgroundColor { get => (Brush)GetValue(BarBackgroundColorProperty); set => SetValue(BarBackgroundColorProperty, value); }

    public Brush BarStrokeColor { get => (Brush)GetValue(BarStrokeColorProperty); set => SetValue(BarStrokeColorProperty, value); }

    public Brush BarShadowBrush { get => (Brush)GetValue(BarShadowBrushProperty); set => SetValue(BarShadowBrushProperty, value); }

    public BottomNavBarSimple()
    {
        InitializeComponent();
    }
}