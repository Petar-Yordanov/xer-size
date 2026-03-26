using System.Windows.Input;

namespace XerSize.Views.Components;

public partial class WorkoutActionBar : ContentView
{
    public static readonly BindableProperty AddExerciseCommandProperty =
        BindableProperty.Create(nameof(AddExerciseCommand), typeof(ICommand), typeof(WorkoutActionBar));

    public static readonly BindableProperty StartWorkoutCommandProperty =
        BindableProperty.Create(nameof(StartWorkoutCommand), typeof(ICommand), typeof(WorkoutActionBar));

    public static readonly BindableProperty MoreCommandProperty =
        BindableProperty.Create(nameof(MoreCommand), typeof(ICommand), typeof(WorkoutActionBar));

    public ICommand? AddExerciseCommand
    {
        get => (ICommand?)GetValue(AddExerciseCommandProperty);
        set => SetValue(AddExerciseCommandProperty, value);
    }

    public ICommand? StartWorkoutCommand
    {
        get => (ICommand?)GetValue(StartWorkoutCommandProperty);
        set => SetValue(StartWorkoutCommandProperty, value);
    }

    public ICommand? MoreCommand
    {
        get => (ICommand?)GetValue(MoreCommandProperty);
        set => SetValue(MoreCommandProperty, value);
    }

    public WorkoutActionBar()
    {
        InitializeComponent();
    }
}
