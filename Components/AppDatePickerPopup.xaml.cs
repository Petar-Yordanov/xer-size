using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using XerSize.Models.Presentation.Calendar;

namespace XerSize.Components;

public partial class AppDatePickerPopup : Popup<DateTime>
{
    private const double MonthPanTriggerDistance = 24;

    private readonly DateTime originalDate;
    private DateTime selectedDate;
    private DateTime visibleMonth;
    private bool isAnimatingMonth;
    private double monthPanTotalX;
    private double monthPanTotalY;
    private bool hasHandledMonthPan;

    public ObservableCollection<CalendarDayPresentationModel> Days { get; } = new();

    public DateTime MinimumDate { get; }

    public DateTime MaximumDate { get; private set; }

    public string DateFormat { get; }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand ApplyCommand { get; }

    public string VisibleMonthText => visibleMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    public string SelectedDateText => selectedDate.ToString(DateFormat, CultureInfo.CurrentCulture);

    public bool CanGoToPreviousMonth => visibleMonth > new DateTime(MinimumDate.Year, MinimumDate.Month, 1);

    public bool CanGoToNextMonth => visibleMonth < new DateTime(MaximumDate.Year, MaximumDate.Month, 1);

    public double PreviousMonthOpacity => CanGoToPreviousMonth ? 1.0 : 0.35;

    public double NextMonthOpacity => CanGoToNextMonth ? 1.0 : 0.35;

    public AppDatePickerPopup(
        DateTime date,
        DateTime minimumDate,
        DateTime maximumDate,
        string dateFormat)
    {
        MinimumDate = minimumDate.Date;
        MaximumDate = maximumDate.Date;

        if (MaximumDate < MinimumDate)
            MaximumDate = MinimumDate;

        DateFormat = string.IsNullOrWhiteSpace(dateFormat)
            ? "dd MMM yyyy"
            : dateFormat;

        selectedDate = ClampDate(date.Date);
        originalDate = selectedDate;
        visibleMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);

        PreviousMonthCommand = new Command(async () => await GoToPreviousMonthAsync());
        NextMonthCommand = new Command(async () => await GoToNextMonthAsync());
        CancelCommand = new Command(async () => await CloseAsync(originalDate));
        ApplyCommand = new Command(async () => await CloseAsync(selectedDate));

        InitializeComponent();
        BuildCalendar();
    }

    private async Task GoToPreviousMonthAsync()
    {
        if (!CanGoToPreviousMonth || isAnimatingMonth)
            return;

        await SlideToMonthAsync(-1);
    }

    private async Task GoToNextMonthAsync()
    {
        if (!CanGoToNextMonth || isAnimatingMonth)
            return;

        await SlideToMonthAsync(1);
    }

    private async Task SlideToMonthAsync(int direction)
    {
        isAnimatingMonth = true;

        try
        {
            var width = DaysCollection.Width > 0 ? DaysCollection.Width : 320;
            var exitX = direction > 0 ? -width : width;
            var entryX = direction > 0 ? width : -width;

            await DaysCollection.TranslateToAsync(exitX, 0, 120, Easing.CubicIn);

            visibleMonth = visibleMonth.AddMonths(direction);
            RefreshCalendarState();

            DaysCollection.TranslationX = entryX;

            await DaysCollection.TranslateToAsync(0, 0, 160, Easing.CubicOut);
        }
        finally
        {
            isAnimatingMonth = false;
        }
    }

    private async void OnCalendarPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (isAnimatingMonth)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                monthPanTotalX = 0;
                monthPanTotalY = 0;
                hasHandledMonthPan = false;
                break;

            case GestureStatus.Running:
                if (hasHandledMonthPan)
                    return;

                monthPanTotalX = e.TotalX;
                monthPanTotalY = e.TotalY;

                var horizontalDistance = Math.Abs(monthPanTotalX);
                var verticalDistance = Math.Abs(monthPanTotalY);

                if (horizontalDistance < MonthPanTriggerDistance)
                    return;

                if (horizontalDistance < verticalDistance * 1.15)
                    return;

                hasHandledMonthPan = true;

                if (monthPanTotalX < 0)
                    await GoToNextMonthAsync();
                else
                    await GoToPreviousMonthAsync();

                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                monthPanTotalX = 0;
                monthPanTotalY = 0;
                hasHandledMonthPan = false;
                break;
        }
    }

    private void OnDayTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not CalendarDayPresentationModel day)
            return;

        if (!day.IsEnabled)
            return;

        selectedDate = day.Date.Date;

        if (selectedDate.Month != visibleMonth.Month || selectedDate.Year != visibleMonth.Year)
            visibleMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);

        RefreshCalendarState();
    }

    private void BuildCalendar()
    {
        Days.Clear();

        var firstOfMonth = new DateTime(visibleMonth.Year, visibleMonth.Month, 1);
        var mondayFirstOffset = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        var gridStart = firstOfMonth.AddDays(-mondayFirstOffset);

        for (var i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i).Date;

            Days.Add(new CalendarDayPresentationModel
            {
                Date = date,
                IsCurrentMonth = date.Month == visibleMonth.Month && date.Year == visibleMonth.Year,
                IsSelected = date == selectedDate.Date,
                IsToday = date == DateTime.Today,
                IsEnabled = date >= MinimumDate && date <= MaximumDate
            });
        }
    }

    private void RefreshCalendarState()
    {
        BuildCalendar();

        OnPropertyChanged(nameof(VisibleMonthText));
        OnPropertyChanged(nameof(SelectedDateText));
        OnPropertyChanged(nameof(CanGoToPreviousMonth));
        OnPropertyChanged(nameof(CanGoToNextMonth));
        OnPropertyChanged(nameof(PreviousMonthOpacity));
        OnPropertyChanged(nameof(NextMonthOpacity));
    }

    private DateTime ClampDate(DateTime date)
    {
        if (date < MinimumDate)
            return MinimumDate;

        if (date > MaximumDate)
            return MaximumDate;

        return date;
    }
}