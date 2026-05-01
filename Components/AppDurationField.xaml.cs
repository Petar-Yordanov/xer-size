using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace XerSize.Components;

public partial class AppDurationField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(AppDurationField),
            string.Empty,
            propertyChanged: OnLabelChanged);

    public static readonly BindableProperty SecondsProperty =
        BindableProperty.Create(
            nameof(Seconds),
            typeof(int),
            typeof(AppDurationField),
            90,
            BindingMode.TwoWay,
            propertyChanged: OnSecondsChanged);

    public static readonly BindableProperty MaximumMinutesProperty =
        BindableProperty.Create(
            nameof(MaximumMinutes),
            typeof(int),
            typeof(AppDurationField),
            10);

    public static readonly BindableProperty FieldBackgroundColorProperty =
        BindableProperty.Create(
            nameof(FieldBackgroundColor),
            typeof(Color),
            typeof(AppDurationField),
            Colors.White);

    public static readonly BindableProperty TextColorValueProperty =
        BindableProperty.Create(
            nameof(TextColorValue),
            typeof(Color),
            typeof(AppDurationField),
            Colors.Black);

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(AppDurationField),
            default(ImageSource),
            propertyChanged: OnIconChanged);

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(
            nameof(IconBackgroundColor),
            typeof(Color),
            typeof(AppDurationField),
            Colors.Transparent);

    public static readonly BindableProperty IconStrokeColorProperty =
        BindableProperty.Create(
            nameof(IconStrokeColor),
            typeof(Color),
            typeof(AppDurationField),
            Colors.Transparent);

    private bool isPopupOpen;

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public int Seconds
    {
        get => (int)GetValue(SecondsProperty);
        set => SetValue(SecondsProperty, Math.Max(0, value));
    }

    public int MaximumMinutes
    {
        get => (int)GetValue(MaximumMinutesProperty);
        set => SetValue(MaximumMinutesProperty, Math.Max(0, value));
    }

    public Color FieldBackgroundColor
    {
        get => (Color)GetValue(FieldBackgroundColorProperty);
        set => SetValue(FieldBackgroundColorProperty, value);
    }

    public Color TextColorValue
    {
        get => (Color)GetValue(TextColorValueProperty);
        set => SetValue(TextColorValueProperty, value);
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

    public Color IconStrokeColor
    {
        get => (Color)GetValue(IconStrokeColorProperty);
        set => SetValue(IconStrokeColorProperty, value);
    }

    public bool HasLabel => !string.IsNullOrWhiteSpace(Label);

    public bool HasIcon => IconSource is not null;

    public string DurationText
    {
        get
        {
            var minutes = Seconds / 60;
            var seconds = Seconds % 60;

            if (minutes <= 0)
                return $"{seconds} sec";

            if (seconds <= 0)
                return $"{minutes} min";

            return $"{minutes} min {seconds} sec";
        }
    }

    public AppDurationField()
    {
        InitializeComponent();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (isPopupOpen)
            return;

        isPopupOpen = true;

        try
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page == null)
                return;

            var popup = new AppDurationPickerPopup(
                Seconds,
                MaximumMinutes);

            var result = await page.ShowPopupAsync<int>(
                popup,
                PopupOptions.Empty,
                CancellationToken.None);

            if (result.WasDismissedByTappingOutsideOfPopup)
                return;

            Seconds = result.Result;
        }
        finally
        {
            isPopupOpen = false;
        }
    }

    private static void OnSecondsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDurationField)bindable).OnPropertyChanged(nameof(DurationText));
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDurationField)bindable).OnPropertyChanged(nameof(HasLabel));
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDurationField)bindable).OnPropertyChanged(nameof(HasIcon));
    }
}