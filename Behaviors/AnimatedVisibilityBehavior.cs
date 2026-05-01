namespace XerSize.Behaviors;

public sealed class AnimatedVisibilityBehavior : Behavior<VisualElement>
{
    private VisualElement? associatedView;
    private double baseTranslationY;
    private int animationVersion;

    public static readonly BindableProperty IsVisibleStateProperty =
        BindableProperty.Create(
            nameof(IsVisibleState),
            typeof(bool),
            typeof(AnimatedVisibilityBehavior),
            false,
            propertyChanged: OnIsVisibleStateChanged);

    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(nameof(Duration), typeof(uint), typeof(AnimatedVisibilityBehavior), 170u);

    public static readonly BindableProperty TranslationYProperty =
        BindableProperty.Create(nameof(TranslationY), typeof(double), typeof(AnimatedVisibilityBehavior), 18d);

    public bool IsVisibleState
    {
        get => (bool)GetValue(IsVisibleStateProperty);
        set => SetValue(IsVisibleStateProperty, value);
    }

    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double TranslationY
    {
        get => (double)GetValue(TranslationYProperty);
        set => SetValue(TranslationYProperty, value);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);

        associatedView = bindable;
        baseTranslationY = bindable.TranslationY;

        bindable.IsVisible = IsVisibleState;
        bindable.Opacity = IsVisibleState ? 1 : 0;

        if (!IsVisibleState)
            bindable.TranslationY = baseTranslationY + TranslationY;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        associatedView = null;

        base.OnDetachingFrom(bindable);
    }

    private static void OnIsVisibleStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var behavior = (AnimatedVisibilityBehavior)bindable;

        if (newValue is bool isVisible)
            behavior.AnimateVisibility(isVisible);
    }

    private async void AnimateVisibility(bool isVisible)
    {
        if (associatedView == null)
            return;

        var version = ++animationVersion;
        var view = associatedView;

        view.AbortAnimation("FadeTo");
        view.AbortAnimation("TranslateTo");

        if (isVisible)
        {
            view.IsVisible = true;
            view.Opacity = 0;
            view.TranslationY = baseTranslationY + TranslationY;

            await Task.WhenAll(
                view.FadeToAsync(1, Duration, Easing.CubicOut),
                view.TranslateToAsync(view.TranslationX, baseTranslationY, Duration, Easing.CubicOut));

            return;
        }

        await Task.WhenAll(
            view.FadeToAsync(0, Duration, Easing.CubicIn),
            view.TranslateToAsync(view.TranslationX, baseTranslationY + TranslationY, Duration, Easing.CubicIn));

        if (version == animationVersion)
            view.IsVisible = false;
    }
}