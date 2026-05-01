using System.Collections;
using System.Windows.Input;

namespace XerSize.Components;

public partial class WorkoutExerciseCard : ContentView
{
    public static readonly BindableProperty ExerciseNameProperty =
        BindableProperty.Create(nameof(ExerciseName), typeof(string), typeof(WorkoutExerciseCard), string.Empty);

    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(WorkoutExerciseCard), default(ImageSource));

    public static readonly BindableProperty SummaryTextProperty =
        BindableProperty.Create(nameof(SummaryText), typeof(string), typeof(WorkoutExerciseCard), string.Empty);

    public static readonly BindableProperty SetsProperty =
        BindableProperty.Create(nameof(Sets), typeof(IEnumerable), typeof(WorkoutExerciseCard), null);

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(WorkoutExerciseCard), false, BindingMode.TwoWay, propertyChanged: OnExpandedChanged);

    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(WorkoutExerciseCard));

    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(WorkoutExerciseCard));

    public static readonly BindableProperty ToggleExpandCommandProperty =
        BindableProperty.Create(nameof(ToggleExpandCommand), typeof(ICommand), typeof(WorkoutExerciseCard));

    public static readonly BindableProperty ReorderCommandProperty =
        BindableProperty.Create(nameof(ReorderCommand), typeof(ICommand), typeof(WorkoutExerciseCard));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(WorkoutExerciseCard));

    public static readonly BindableProperty ProgressCommandProperty =
    BindableProperty.Create(nameof(ProgressCommand), typeof(ICommand), typeof(WorkoutExerciseCard));

    public ICommand? ProgressCommand
    {
        get => (ICommand?)GetValue(ProgressCommandProperty);
        set => SetValue(ProgressCommandProperty, value);
    }

    public string ExerciseName
    {
        get => (string)GetValue(ExerciseNameProperty);
        set => SetValue(ExerciseNameProperty, value);
    }

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string SummaryText
    {
        get => (string)GetValue(SummaryTextProperty);
        set => SetValue(SummaryTextProperty, value);
    }

    public IEnumerable? Sets
    {
        get => (IEnumerable?)GetValue(SetsProperty);
        set => SetValue(SetsProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public ICommand? ToggleExpandCommand
    {
        get => (ICommand?)GetValue(ToggleExpandCommandProperty);
        set => SetValue(ToggleExpandCommandProperty, value);
    }

    public ICommand? ReorderCommand
    {
        get => (ICommand?)GetValue(ReorderCommandProperty);
        set => SetValue(ReorderCommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string ExpandIconSource => IsExpanded ? "expand_less.png" : "expand_more.png";

    public WorkoutExerciseCard()
    {
        InitializeComponent();
    }

    private static void OnExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((WorkoutExerciseCard)bindable).OnPropertyChanged(nameof(ExpandIconSource));
    }
}