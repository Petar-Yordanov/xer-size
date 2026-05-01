namespace XerSize.Behaviors;

public sealed class DragItemBehavior : Behavior<View>
{
    private const string DraggedItemKey = "XerSize.DraggedWorkoutExercise";

    private readonly DragGestureRecognizer dragGestureRecognizer = new();

    private View? associatedView;

    internal static object? CurrentDraggedItem { get; private set; }

    internal static string DraggedItemDataKey => DraggedItemKey;

    internal static void ClearDraggedItem()
    {
        CurrentDraggedItem = null;
    }

    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(nameof(Item), typeof(object), typeof(DragItemBehavior));

    public static readonly BindableProperty DragScaleProperty =
        BindableProperty.Create(nameof(DragScale), typeof(double), typeof(DragItemBehavior), 1.04d);

    public static readonly BindableProperty DragOpacityProperty =
        BindableProperty.Create(nameof(DragOpacity), typeof(double), typeof(DragItemBehavior), 0.82d);

    public static readonly BindableProperty DragTranslationYProperty =
        BindableProperty.Create(nameof(DragTranslationY), typeof(double), typeof(DragItemBehavior), -8d);

    public object? Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public double DragScale
    {
        get => (double)GetValue(DragScaleProperty);
        set => SetValue(DragScaleProperty, value);
    }

    public double DragOpacity
    {
        get => (double)GetValue(DragOpacityProperty);
        set => SetValue(DragOpacityProperty, value);
    }

    public double DragTranslationY
    {
        get => (double)GetValue(DragTranslationYProperty);
        set => SetValue(DragTranslationYProperty, value);
    }

    public DragItemBehavior()
    {
        dragGestureRecognizer.CanDrag = true;
        dragGestureRecognizer.DragStarting += OnDragStarting;
        dragGestureRecognizer.DropCompleted += OnDropCompleted;
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        associatedView = bindable;
        bindable.GestureRecognizers.Add(dragGestureRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        bindable.GestureRecognizers.Remove(dragGestureRecognizer);
        associatedView = null;

        base.OnDetachingFrom(bindable);
    }

    private async void OnDragStarting(object? sender, DragStartingEventArgs e)
    {
        CurrentDraggedItem = Item;

        if (Item != null)
            e.Data.Properties[DraggedItemKey] = Item;

        if (associatedView == null)
            return;

        associatedView.Opacity = DragOpacity;
        associatedView.TranslationY = DragTranslationY;

        await associatedView.ScaleToAsync(DragScale, 80, Easing.CubicOut);
    }

    private async void OnDropCompleted(object? sender, DropCompletedEventArgs e)
    {
        if (associatedView != null)
        {
            await associatedView.TranslateToAsync(0, 0, 80, Easing.CubicOut);
            await associatedView.ScaleToAsync(1, 80, Easing.CubicOut);

            associatedView.Opacity = 1;
        }

        associatedView?.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(350),
            ClearDraggedItem);
    }
}