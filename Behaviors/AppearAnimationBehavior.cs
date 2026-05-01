namespace XerSize.Behaviors;

public sealed class AppearAnimationBehavior : Behavior<VisualElement>
{
    private VisualElement? associatedView;
    private bool hasPlayed;
    private double baseOpacity = 1;
    private double baseTranslationY;

    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(nameof(Duration), typeof(uint), typeof(AppearAnimationBehavior), 240u);

    public static readonly BindableProperty DelayProperty =
        BindableProperty.Create(nameof(Delay), typeof(uint), typeof(AppearAnimationBehavior), 0u);

    public static readonly BindableProperty TranslationYProperty =
        BindableProperty.Create(nameof(TranslationY), typeof(double), typeof(AppearAnimationBehavior), 14d);

    public static readonly BindableProperty InitialOpacityProperty =
        BindableProperty.Create(nameof(InitialOpacity), typeof(double), typeof(AppearAnimationBehavior), 0d);

    public static readonly BindableProperty OnlyOnceProperty =
        BindableProperty.Create(nameof(OnlyOnce), typeof(bool), typeof(AppearAnimationBehavior), true);

    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public uint Delay
    {
        get => (uint)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public double TranslationY
    {
        get => (double)GetValue(TranslationYProperty);
        set => SetValue(TranslationYProperty, value);
    }

    public double InitialOpacity
    {
        get => (double)GetValue(InitialOpacityProperty);
        set => SetValue(InitialOpacityProperty, value);
    }

    public bool OnlyOnce
    {
        get => (bool)GetValue(OnlyOnceProperty);
        set => SetValue(OnlyOnceProperty, value);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);

        associatedView = bindable;
        baseOpacity = bindable.Opacity;
        baseTranslationY = bindable.TranslationY;

        bindable.Loaded += OnLoaded;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.Loaded -= OnLoaded;
        associatedView = null;

        base.OnDetachingFrom(bindable);
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (associatedView == null)
            return;

        if (OnlyOnce && hasPlayed)
            return;

        hasPlayed = true;

        associatedView.Opacity = InitialOpacity;
        associatedView.TranslationY = baseTranslationY + TranslationY;

        if (Delay > 0)
            await Task.Delay((int)Delay);

        await Task.WhenAll(
            associatedView.FadeToAsync(baseOpacity, Duration, Easing.CubicOut),
            associatedView.TranslateToAsync(associatedView.TranslationX, baseTranslationY, Duration, Easing.CubicOut));
    }
}