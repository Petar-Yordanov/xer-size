using System.Windows.Input;
using XerSize.Behaviors;

namespace XerSize.Components;

public partial class TabPill : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TabPill), string.Empty);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TabPill), false);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TabPill));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TabPill));

    public static readonly BindableProperty ReorderCommandProperty =
        BindableProperty.Create(nameof(ReorderCommand), typeof(ICommand), typeof(TabPill), propertyChanged: OnReorderChanged);

    public static readonly BindableProperty EnableReorderProperty =
        BindableProperty.Create(nameof(EnableReorder), typeof(bool), typeof(TabPill), false, propertyChanged: OnReorderChanged);

    private DropReorderBehavior? dropReorderBehavior;
    private DragItemBehavior? dragItemBehavior;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ICommand? ReorderCommand
    {
        get => (ICommand?)GetValue(ReorderCommandProperty);
        set => SetValue(ReorderCommandProperty, value);
    }

    public bool EnableReorder
    {
        get => (bool)GetValue(EnableReorderProperty);
        set => SetValue(EnableReorderProperty, value);
    }

    public TabPill()
    {
        InitializeComponent();
        SyncReorderBehaviors();
    }

    private static void OnReorderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((TabPill)bindable).SyncReorderBehaviors();
    }

    private void SyncReorderBehaviors()
    {
        if (RootGrid is null)
            return;

        if (!EnableReorder || ReorderCommand is null)
        {
            RemoveReorderBehaviors();
            return;
        }

        if (dropReorderBehavior is not null && dragItemBehavior is not null)
            return;

        dropReorderBehavior = new DropReorderBehavior();
        dropReorderBehavior.SetBinding(
            DropReorderBehavior.CommandProperty,
            new Binding(nameof(ReorderCommand), source: this));
        dropReorderBehavior.SetBinding(
            DropReorderBehavior.TargetItemProperty,
            new Binding(nameof(CommandParameter), source: this));

        dragItemBehavior = new DragItemBehavior
        {
            DragScale = 1,
            DragOpacity = 1,
            DragTranslationY = 0
        };
        dragItemBehavior.SetBinding(
            DragItemBehavior.ItemProperty,
            new Binding(nameof(CommandParameter), source: this));

        RootGrid.Behaviors.Add(dropReorderBehavior);
        RootGrid.Behaviors.Add(dragItemBehavior);
    }

    private void RemoveReorderBehaviors()
    {
        if (RootGrid is null)
            return;

        if (dropReorderBehavior is not null)
        {
            RootGrid.Behaviors.Remove(dropReorderBehavior);
            dropReorderBehavior = null;
        }

        if (dragItemBehavior is not null)
        {
            RootGrid.Behaviors.Remove(dragItemBehavior);
            dragItemBehavior = null;
        }
    }
}