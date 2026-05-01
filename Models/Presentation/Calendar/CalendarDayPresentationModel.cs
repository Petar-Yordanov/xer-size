namespace XerSize.Models.Presentation.Calendar;

public sealed class CalendarDayPresentationModel
{
    public DateTime Date { get; set; }

    public string DayText => Date.Day.ToString();

    public bool IsCurrentMonth { get; set; }

    public bool IsSelected { get; set; }

    public bool IsToday { get; set; }

    public bool IsEnabled { get; set; }
}