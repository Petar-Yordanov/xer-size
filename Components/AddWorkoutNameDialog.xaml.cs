using System.Windows.Input;

#if ANDROID || IOS
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace XerSize.Components;

public partial class AddWorkoutNameDialog : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(AddWorkoutNameDialog),
            false,
            propertyChanged: OnIsOpenChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AddWorkoutNameDialog), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            string.Empty,
            propertyChanged: OnMessageChanged);

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(AddWorkoutNameDialog),
            default(ImageSource),
            propertyChanged: OnIconChanged);

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(
            nameof(IconBackgroundColor),
            typeof(Color),
            typeof(AddWorkoutNameDialog),
            Colors.Transparent);

    public static readonly BindableProperty PrimaryButtonTextProperty =
        BindableProperty.Create(
            nameof(PrimaryButtonText),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            "Create");

    public static readonly BindableProperty SecondaryButtonTextProperty =
        BindableProperty.Create(
            nameof(SecondaryButtonText),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            "Cancel");

    public static readonly BindableProperty PrimaryCommandProperty =
        BindableProperty.Create(
            nameof(PrimaryCommand),
            typeof(ICommand),
            typeof(AddWorkoutNameDialog));

    public static readonly BindableProperty SecondaryCommandProperty =
        BindableProperty.Create(
            nameof(SecondaryCommand),
            typeof(ICommand),
            typeof(AddWorkoutNameDialog));

    public static readonly BindableProperty DismissCommandProperty =
        BindableProperty.Create(
            nameof(DismissCommand),
            typeof(ICommand),
            typeof(AddWorkoutNameDialog));

    public static readonly BindableProperty WorkoutNameProperty =
        BindableProperty.Create(
            nameof(WorkoutName),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            string.Empty,
            BindingMode.TwoWay);

    public static readonly BindableProperty FieldLabelProperty =
        BindableProperty.Create(
            nameof(FieldLabel),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            "Workout name");

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(AddWorkoutNameDialog),
            "Example: Push A");

    private Entry? WorkoutNameEntryControl => this.FindByName<Entry>("WorkoutNameEntry");

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public string SecondaryButtonText
    {
        get => (string)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public ICommand? PrimaryCommand
    {
        get => (ICommand?)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public ICommand? SecondaryCommand
    {
        get => (ICommand?)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public ICommand? DismissCommand
    {
        get => (ICommand?)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public string WorkoutName
    {
        get => (string)GetValue(WorkoutNameProperty);
        set => SetValue(WorkoutNameProperty, value);
    }

    public string FieldLabel
    {
        get => (string)GetValue(FieldLabelProperty);
        set => SetValue(FieldLabelProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool ShowIcon => IconSource is not null;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public ICommand InternalPrimaryCommand { get; }

    public ICommand InternalSecondaryCommand { get; }

    public ICommand InternalDismissCommand { get; }

    public AddWorkoutNameDialog()
    {
        InternalPrimaryCommand = new Command(async () => await ExecutePrimaryAsync());
        InternalSecondaryCommand = new Command(async () => await ExecuteSecondaryAsync());
        InternalDismissCommand = new Command(async () => await ExecuteDismissAsync());

        InitializeComponent();
    }

    private async Task ExecutePrimaryAsync()
    {
        await HideKeyboardAsync();

        if (PrimaryCommand?.CanExecute(null) == true)
            PrimaryCommand.Execute(null);
    }

    private async Task ExecuteSecondaryAsync()
    {
        await HideKeyboardAsync();

        if (SecondaryCommand?.CanExecute(null) == true)
            SecondaryCommand.Execute(null);
    }

    private async Task ExecuteDismissAsync()
    {
        await HideKeyboardAsync();

        if (DismissCommand?.CanExecute(null) == true)
            DismissCommand.Execute(null);
    }

    private async Task HideKeyboardAsync()
    {
        var entry = WorkoutNameEntryControl;

        if (entry == null)
            return;

        try
        {
            entry.Unfocus();

#if ANDROID || IOS
            if (entry.IsSoftInputShowing())
                await entry.HideKeyboardAsync(CancellationToken.None);
#endif
        }
        catch
        {
            entry.Unfocus();
        }
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var dialog = (AddWorkoutNameDialog)bindable;

        if (newValue is not true)
            return;

        _ = dialog.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(190),
            () => dialog.WorkoutNameEntryControl?.Focus());
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AddWorkoutNameDialog)bindable).OnPropertyChanged(nameof(ShowIcon));
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AddWorkoutNameDialog)bindable).OnPropertyChanged(nameof(HasMessage));
    }
}