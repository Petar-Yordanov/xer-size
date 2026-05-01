using System.Windows.Input;
using XerSize.Models.Presentation.Workouts;

namespace XerSize.Behaviors;

public sealed class DropReorderBehavior : Behavior<View>
{
    private readonly DropGestureRecognizer dropGestureRecognizer = new();

    public static readonly BindableProperty TargetItemProperty =
        BindableProperty.Create(nameof(TargetItem), typeof(object), typeof(DropReorderBehavior));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(DropReorderBehavior));

    public object? TargetItem
    {
        get => GetValue(TargetItemProperty);
        set => SetValue(TargetItemProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public DropReorderBehavior()
    {
        dropGestureRecognizer.AllowDrop = true;
        dropGestureRecognizer.Drop += OnDrop;
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        bindable.GestureRecognizers.Add(dropGestureRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        bindable.GestureRecognizers.Remove(dropGestureRecognizer);

        base.OnDetachingFrom(bindable);
    }

    private void OnDrop(object? sender, DropEventArgs e)
    {
        var sourceItem = GetSourceItem(e);
        var targetItem = TargetItem;

        if (sourceItem == null || targetItem == null || ReferenceEquals(sourceItem, targetItem))
            return;

        var request = new WorkoutExerciseReorderRequest
        {
            SourceItem = sourceItem,
            TargetItem = targetItem
        };

        if (Command?.CanExecute(request) == true)
        {
            Command.Execute(request);
            DragItemBehavior.ClearDraggedItem();
        }
    }

    private static object? GetSourceItem(DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue(DragItemBehavior.DraggedItemDataKey, out var itemFromData))
            return itemFromData;

        return DragItemBehavior.CurrentDraggedItem;
    }
}