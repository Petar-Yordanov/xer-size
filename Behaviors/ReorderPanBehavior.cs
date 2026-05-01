using System.Windows.Input;
using XerSize.Models.Presentation.Workouts;

namespace XerSize.Behaviors;

public sealed class ReorderPanBehavior : Behavior<View>
{
    private double totalY;
    private bool isDragging;

    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(nameof(Item), typeof(object), typeof(ReorderPanBehavior));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ReorderPanBehavior));

    public static readonly BindableProperty ThresholdProperty =
        BindableProperty.Create(nameof(Threshold), typeof(double), typeof(ReorderPanBehavior), 72d);

    public object? Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public double Threshold
    {
        get => (double)GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        var recognizer = new PanGestureRecognizer();
        recognizer.PanUpdated += OnPanUpdated;

        bindable.GestureRecognizers.Add(recognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        var recognizers = bindable.GestureRecognizers
            .OfType<PanGestureRecognizer>()
            .ToList();

        foreach (var recognizer in recognizers)
        {
            recognizer.PanUpdated -= OnPanUpdated;
            bindable.GestureRecognizers.Remove(recognizer);
        }

        base.OnDetachingFrom(bindable);
    }

    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (sender is not View view)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                totalY = 0;
                isDragging = true;

                await view.ScaleToAsync(1.03, 80, Easing.CubicOut);
                await view.FadeToAsync(0.86, 80, Easing.CubicOut);
                view.ZIndex = 10;
                break;

            case GestureStatus.Running:
                if (!isDragging)
                    return;

                totalY = e.TotalY;
                view.TranslationY = totalY;

                TrySendReorderRequest();
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                isDragging = false;
                totalY = 0;

                await view.TranslateToAsync(0, 0, 120, Easing.CubicOut);
                await view.ScaleToAsync(1, 100, Easing.CubicOut);
                await view.FadeToAsync(1, 100, Easing.CubicOut);
                view.ZIndex = 0;
                break;
        }
    }

    private void TrySendReorderRequest()
    {
        if (Item is null)
            return;

        var threshold = Math.Max(1, Threshold);

        if (Math.Abs(totalY) < threshold)
            return;

        var direction = totalY > 0 ? 1 : -1;

        var request = new WorkoutExerciseReorderRequest
        {
            Item = Item,
            Direction = direction
        };

        if (Command?.CanExecute(request) == true)
            Command.Execute(request);

        totalY = 0;
    }
}