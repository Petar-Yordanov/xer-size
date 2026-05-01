using System.Windows.Input;
using CommunityToolkit.Maui.Views;

namespace XerSize.Components;

public partial class AppDurationPickerPopup : Popup<int>
{
    private readonly int originalSeconds;
    private readonly int maximumMinutes;

    private int minutes;
    private int seconds;

    public ICommand IncreaseMinutesCommand { get; }
    public ICommand DecreaseMinutesCommand { get; }
    public ICommand IncreaseSecondsCommand { get; }
    public ICommand DecreaseSecondsCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ApplyCommand { get; }

    public string MinutesText => minutes.ToString("00");

    public string SecondsText => seconds.ToString("00");

    public string DurationText
    {
        get
        {
            if (minutes <= 0)
                return $"{seconds} sec";

            if (seconds <= 0)
                return $"{minutes} min";

            return $"{minutes} min {seconds} sec";
        }
    }

    public AppDurationPickerPopup(int totalSeconds, int maximumMinutes)
    {
        originalSeconds = Math.Max(0, totalSeconds);
        this.maximumMinutes = Math.Max(0, maximumMinutes);

        minutes = Math.Min(this.maximumMinutes, originalSeconds / 60);
        seconds = originalSeconds % 60;

        IncreaseMinutesCommand = new Command(IncreaseMinutes);
        DecreaseMinutesCommand = new Command(DecreaseMinutes);
        IncreaseSecondsCommand = new Command(IncreaseSeconds);
        DecreaseSecondsCommand = new Command(DecreaseSeconds);
        CancelCommand = new Command(async () => await CloseAsync(originalSeconds));
        ApplyCommand = new Command(async () => await CloseAsync(ToTotalSeconds()));

        InitializeComponent();
    }

    private void IncreaseMinutes()
    {
        if (minutes >= maximumMinutes)
            return;

        minutes++;
        Refresh();
    }

    private void DecreaseMinutes()
    {
        if (minutes <= 0)
            return;

        minutes--;
        Refresh();
    }

    private void IncreaseSeconds()
    {
        seconds += 5;

        if (seconds >= 60)
        {
            seconds = 0;

            if (minutes < maximumMinutes)
                minutes++;
            else
                minutes = maximumMinutes;
        }

        Refresh();
    }

    private void DecreaseSeconds()
    {
        seconds -= 5;

        if (seconds < 0)
        {
            if (minutes > 0)
            {
                minutes--;
                seconds = 55;
            }
            else
            {
                seconds = 0;
            }
        }

        Refresh();
    }

    private int ToTotalSeconds()
    {
        return Math.Max(0, (minutes * 60) + seconds);
    }

    private void Refresh()
    {
        OnPropertyChanged(nameof(MinutesText));
        OnPropertyChanged(nameof(SecondsText));
        OnPropertyChanged(nameof(DurationText));
    }
}