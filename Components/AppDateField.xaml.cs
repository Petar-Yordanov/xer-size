using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace XerSize.Components;

public partial class AppDateField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(AppDateField),
            string.Empty,
            propertyChanged: OnLabelChanged);

    public static readonly BindableProperty DateProperty =
        BindableProperty.Create(
            nameof(Date),
            typeof(DateTime),
            typeof(AppDateField),
            DateTime.Today,
            BindingMode.TwoWay,
            propertyChanged: OnDateChanged);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(
            nameof(MinimumDate),
            typeof(DateTime),
            typeof(AppDateField),
            new DateTime(1900, 1, 1));

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(
            nameof(MaximumDate),
            typeof(DateTime),
            typeof(AppDateField),
            new DateTime(2100, 12, 31));

    public static readonly BindableProperty DateFormatProperty =
        BindableProperty.Create(
            nameof(DateFormat),
            typeof(string),
            typeof(AppDateField),
            "dd MMM yyyy",
            propertyChanged: OnDateFormatChanged);

    public static readonly BindableProperty FieldBackgroundColorProperty =
        BindableProperty.Create(
            nameof(FieldBackgroundColor),
            typeof(Color),
            typeof(AppDateField),
            Colors.White);

    public static readonly BindableProperty TextColorValueProperty =
        BindableProperty.Create(
            nameof(TextColorValue),
            typeof(Color),
            typeof(AppDateField),
            Colors.Black);

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(AppDateField),
            default(ImageSource),
            propertyChanged: OnIconChanged);

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(
            nameof(IconBackgroundColor),
            typeof(Color),
            typeof(AppDateField),
            Colors.Transparent);

    public static readonly BindableProperty IconStrokeColorProperty =
        BindableProperty.Create(
            nameof(IconStrokeColor),
            typeof(Color),
            typeof(AppDateField),
            Colors.Transparent);

    private bool isPopupOpen;

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public DateTime MinimumDate
    {
        get => (DateTime)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public string DateFormat
    {
        get => (string)GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
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

    public string DateText => Date.ToString(DateFormat, CultureInfo.CurrentCulture);

    public bool HasLabel => !string.IsNullOrWhiteSpace(Label);

    public bool HasIcon => IconSource is not null;

    public AppDateField()
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

            var popup = new AppDatePickerPopup(
                Date,
                MinimumDate,
                MaximumDate,
                DateFormat);

            var result = await page.ShowPopupAsync<DateTime>(
                popup,
                PopupOptions.Empty,
                CancellationToken.None);

            if (result.WasDismissedByTappingOutsideOfPopup)
                return;

            Date = result.Result.Date;
        }
        finally
        {
            isPopupOpen = false;
        }
    }

    private static void OnDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDateField)bindable).OnPropertyChanged(nameof(DateText));
    }

    private static void OnDateFormatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDateField)bindable).OnPropertyChanged(nameof(DateText));
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDateField)bindable).OnPropertyChanged(nameof(HasLabel));
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AppDateField)bindable).OnPropertyChanged(nameof(HasIcon));
    }
}