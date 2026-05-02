using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Navigation;
using XerSize.Models.Presentation.Settings;
using XerSize.Repositories.Common;
using XerSize.Services;

namespace XerSize.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    private readonly UserSettingsService userSettingsService;
    private readonly SqliteLocalStore sqliteLocalStore;
    private bool isLoading;

    public SettingsPageViewModel(
        UserSettingsService userSettingsService,
        SqliteLocalStore sqliteLocalStore)
    {
        this.userSettingsService = userSettingsService;
        this.sqliteLocalStore = sqliteLocalStore;

        GenderOptions = new ObservableCollection<string>(
            ProfilePresentationOptions.Genders.Select(ProfilePresentationOptions.ToDisplayName));

        WeeklyGoalOptions = new ObservableCollection<string>(
            ProfilePresentationOptions.WeeklyGoalSessions.Select(ProfilePresentationOptions.ToWeeklyGoalDisplayName));

        UnitOptions = new ObservableCollection<string>(
            ProfilePresentationOptions.Units.Select(ProfilePresentationOptions.ToDisplayName));

        InitialPageOptions = new ObservableCollection<string>(
            ProfilePresentationOptions.InitialPages.Select(ProfilePresentationOptions.ToDisplayName));

        ThemeOptions = new ObservableCollection<string>(
            ProfilePresentationOptions.Themes.Select(ProfilePresentationOptions.ToDisplayName));

        DashboardNavItem = new BottomNavItemPresentationModel
        {
            Id = "dashboard",
            Name = "Home",
            IconSource = "statistics.png"
        };

        WorkoutsNavItem = new BottomNavItemPresentationModel
        {
            Id = "workouts",
            Name = "Workouts",
            IconSource = "exercises.png"
        };

        CalendarNavItem = new BottomNavItemPresentationModel
        {
            Id = "history",
            Name = "History",
            IconSource = "history.png"
        };

        SettingsNavItem = new BottomNavItemPresentationModel
        {
            Id = "settings",
            Name = "Settings",
            IconSource = "settings.png",
            IsSelected = true
        };

        LoadSettings();
        SyncSelectedNav();
    }

    [ObservableProperty]
    public partial string Age { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Height { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Weight { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GenderValue))]
    public partial string SelectedGender { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeeklyGoalValue))]
    public partial string SelectedWeeklyGoal { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeeklyCalorieTargetValue))]
    public partial string WeeklyCalorieTarget { get; set; } = "2000";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitsValue))]
    public partial string SelectedUnits { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialPageValue))]
    public partial string SelectedInitialPage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeValue))]
    public partial string SelectedTheme { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool KeepScreenAwakeDuringWorkout { get; set; }

    [ObservableProperty]
    public partial bool AutoExpandExerciseCards { get; set; }

    [ObservableProperty]
    public partial bool RestTimerSoundEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsProfileExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsBehaviorExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsAppearanceExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsNotificationsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsDataExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsGenderExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsWeeklyGoalExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsUnitsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsInitialPageExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsThemeExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsInfoDialogVisible { get; set; }

    [ObservableProperty]
    public partial string InfoDialogTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoDialogMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoDialogIconSource { get; set; } = "settings.png";

    [ObservableProperty]
    public partial bool IsDataDialogVisible { get; set; }

    [ObservableProperty]
    public partial string DataDialogTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DataDialogMessage { get; set; } = string.Empty;

    public BottomNavItemPresentationModel DashboardNavItem { get; }

    public BottomNavItemPresentationModel WorkoutsNavItem { get; }

    public BottomNavItemPresentationModel CalendarNavItem { get; }

    public BottomNavItemPresentationModel SettingsNavItem { get; }

    public ObservableCollection<string> GenderOptions { get; }

    public ObservableCollection<string> WeeklyGoalOptions { get; }

    public ObservableCollection<string> UnitOptions { get; }

    public ObservableCollection<string> InitialPageOptions { get; }

    public ObservableCollection<string> ThemeOptions { get; }

    public string WeeklyGoalValue => string.IsNullOrWhiteSpace(SelectedWeeklyGoal)
        ? "Not set"
        : SelectedWeeklyGoal;

    public string WeeklyCalorieTargetValue => $"{ParseWeeklyCalorieTarget(WeeklyCalorieTarget)} kcal";

    public string UnitsValue => string.IsNullOrWhiteSpace(SelectedUnits)
        ? "Metric"
        : SelectedUnits;

    public string GenderValue => string.IsNullOrWhiteSpace(SelectedGender)
        ? "Male"
        : SelectedGender;

    public string InitialPageValue => string.IsNullOrWhiteSpace(SelectedInitialPage)
        ? "Dashboard"
        : SelectedInitialPage;

    public string ThemeValue => string.IsNullOrWhiteSpace(SelectedTheme)
        ? "System"
        : SelectedTheme;

    public void SyncSelectedNav()
    {
        DashboardNavItem.IsSelected = false;
        WorkoutsNavItem.IsSelected = false;
        CalendarNavItem.IsSelected = false;
        SettingsNavItem.IsSelected = true;

        OnPropertyChanged(nameof(DashboardNavItem));
        OnPropertyChanged(nameof(WorkoutsNavItem));
        OnPropertyChanged(nameof(CalendarNavItem));
        OnPropertyChanged(nameof(SettingsNavItem));
    }

    partial void OnAgeChanged(string value)
    {
        SaveSettings();
    }

    partial void OnHeightChanged(string value)
    {
        SaveSettings();
    }

    partial void OnWeightChanged(string value)
    {
        SaveSettings();
    }

    partial void OnSelectedGenderChanged(string value)
    {
        SaveSettings();
    }

    partial void OnSelectedWeeklyGoalChanged(string value)
    {
        SaveSettings();
    }

    partial void OnWeeklyCalorieTargetChanged(string value)
    {
        SaveSettings();
    }

    partial void OnSelectedUnitsChanged(string value)
    {
        SaveSettings();
    }

    partial void OnSelectedInitialPageChanged(string value)
    {
        SaveSettings();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        SaveSettings();
    }

    partial void OnKeepScreenAwakeDuringWorkoutChanged(bool value)
    {
        SaveSettings();
    }

    partial void OnAutoExpandExerciseCardsChanged(bool value)
    {
        SaveSettings();
    }

    partial void OnRestTimerSoundEnabledChanged(bool value)
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        isLoading = true;

        var settings = userSettingsService.GetOrCreate();

        Age = settings.Age;
        Height = settings.Height;
        Weight = settings.Weight;
        SelectedGender = ProfilePresentationOptions.ToDisplayName(settings.Gender);
        SelectedWeeklyGoal = ProfilePresentationOptions.ToWeeklyGoalDisplayName(settings.WeeklyGoalSessions);
        WeeklyCalorieTarget = Math.Max(0, settings.WeeklyCalorieTarget).ToString(CultureInfo.InvariantCulture);
        SelectedUnits = ProfilePresentationOptions.ToDisplayName(settings.Units);
        SelectedInitialPage = ProfilePresentationOptions.ToDisplayName(settings.InitialPage);
        SelectedTheme = ProfilePresentationOptions.ToDisplayName(settings.Theme);
        KeepScreenAwakeDuringWorkout = settings.KeepScreenAwakeDuringWorkout;
        AutoExpandExerciseCards = settings.AutoExpandExerciseCards;
        RestTimerSoundEnabled = settings.RestTimerSoundEnabled;

        isLoading = false;

        userSettingsService.UpdateAppearance(settings.Theme);
    }

    private void SaveSettings()
    {
        if (isLoading)
            return;

        userSettingsService.UpdateProfile(
            Age,
            Height,
            Weight,
            ParseGender(SelectedGender),
            ParseWeeklyGoalSessions(SelectedWeeklyGoal),
            ParseWeeklyCalorieTarget(WeeklyCalorieTarget),
            ParseUnitSystem(SelectedUnits),
            ParseInitialPage(SelectedInitialPage));

        userSettingsService.UpdateAppearance(ParseTheme(SelectedTheme));

        userSettingsService.UpdatePreferences(
            KeepScreenAwakeDuringWorkout,
            AutoExpandExerciseCards,
            RestTimerSoundEnabled);
    }

    [RelayCommand]
    private void SelectGender(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SelectedGender = value;
        IsGenderExpanded = false;
    }

    [RelayCommand]
    private void SelectWeeklyGoal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SelectedWeeklyGoal = value;
        IsWeeklyGoalExpanded = false;
    }

    [RelayCommand]
    private void SelectUnits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SelectedUnits = value;
        IsUnitsExpanded = false;
    }

    [RelayCommand]
    private void SelectInitialPage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SelectedInitialPage = value;
        IsInitialPageExpanded = false;
    }

    [RelayCommand]
    private void SelectTheme(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SelectedTheme = value;
        IsThemeExpanded = false;
    }

    [RelayCommand]
    private void ToggleKeepScreenAwake()
    {
        KeepScreenAwakeDuringWorkout = !KeepScreenAwakeDuringWorkout;
    }

    [RelayCommand]
    private void ToggleAutoExpandExerciseCards()
    {
        AutoExpandExerciseCards = !AutoExpandExerciseCards;
    }

    [RelayCommand]
    private void ToggleRestTimerSound()
    {
        RestTimerSoundEnabled = !RestTimerSoundEnabled;
    }

    [RelayCommand]
    private async Task SelectNav(BottomNavItemPresentationModel? item)
    {
        if (item is null)
            return;

        if (item.Id == "dashboard")
            await Shell.Current.GoToAsync($"//{AppShell.DashboardRoute}", true);
        else if (item.Id == "workouts")
            await Shell.Current.GoToAsync(AppShell.WorkoutsRoute, true);
        else if (item.Id == "history")
            await Shell.Current.GoToAsync(AppShell.HistoryRoute, true);
        else if (item.Id == "settings")
            SyncSelectedNav();
    }

    [RelayCommand]
    private async Task ImportData()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose XerSize database backup",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, ["application/octet-stream", "application/x-sqlite3", "application/vnd.sqlite3"] },
                    { DevicePlatform.iOS, ["public.database", "public.data"] },
                    { DevicePlatform.MacCatalyst, ["public.database", "public.data"] },
                    { DevicePlatform.WinUI, [".db3", ".sqlite", ".sqlite3"] }
                })
            });

            if (result is null)
                return;

            var backupPath = await sqliteLocalStore.ReplaceDatabaseAsync(result.FullPath);

            ShowDataDialog(
                "Import complete",
                $"Your database was imported. A backup of the previous database was saved here:\n\n{backupPath}\n\nRestart the app to reload all data cleanly.");
        }
        catch (Exception ex)
        {
            ShowDataDialog(
                "Import failed",
                $"Could not import the selected database.\n\n{ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportData()
    {
        try
        {
            var exportPath = await sqliteLocalStore.CreateExportCopyAsync();

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export XerSize database",
                File = new ShareFile(exportPath)
            });
        }
        catch (Exception ex)
        {
            ShowDataDialog(
                "Export failed",
                $"Could not export the database.\n\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void CloseDataDialog()
    {
        IsDataDialogVisible = false;
    }

    [RelayCommand]
    private void CloseInfoDialog()
    {
        IsInfoDialogVisible = false;
    }

    private void ShowDataDialog(string title, string message)
    {
        DataDialogTitle = title;
        DataDialogMessage = message;
        IsDataDialogVisible = true;
    }

    private static GenderOption ParseGender(string? value)
    {
        return ParseDisplayName(
            value,
            ProfilePresentationOptions.Genders,
            ProfilePresentationOptions.ToDisplayName,
            GenderOption.Male);
    }

    private static UnitSystem ParseUnitSystem(string? value)
    {
        return ParseDisplayName(
            value,
            ProfilePresentationOptions.Units,
            ProfilePresentationOptions.ToDisplayName,
            UnitSystem.Metric);
    }

    private static InitialPageOption ParseInitialPage(string? value)
    {
        return ParseDisplayName(
            value,
            ProfilePresentationOptions.InitialPages,
            ProfilePresentationOptions.ToDisplayName,
            InitialPageOption.Dashboard);
    }

    private static AppThemeOption ParseTheme(string? value)
    {
        return ParseDisplayName(
            value,
            ProfilePresentationOptions.Themes,
            ProfilePresentationOptions.ToDisplayName,
            AppThemeOption.System);
    }

    private static int ParseWeeklyGoalSessions(string? value)
    {
        foreach (var sessions in ProfilePresentationOptions.WeeklyGoalSessions)
        {
            if (string.Equals(
                    ProfilePresentationOptions.ToWeeklyGoalDisplayName(sessions),
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sessions;
            }
        }

        return 3;
    }

    private static int ParseWeeklyCalorieTarget(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return int.TryParse(
            value.Trim(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var calories)
            ? Math.Max(0, calories)
            : 0;
    }

    private static TEnum ParseDisplayName<TEnum>(
        string? value,
        IEnumerable<TEnum> options,
        Func<TEnum, string> displayNameSelector,
        TEnum fallback)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        foreach (var option in options)
        {
            if (Normalize(option.ToString()) == Normalize(value))
                return option;

            if (Normalize(displayNameSelector(option)) == Normalize(value))
                return option;
        }

        return fallback;
    }

    private static string Normalize(string value)
    {
        return new string(
            value
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
    }
}