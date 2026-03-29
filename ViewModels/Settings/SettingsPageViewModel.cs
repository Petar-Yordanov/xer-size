using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using XerSize.Services.Interfaces;
using XerSize.ViewModels.Base;

namespace XerSize.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    private const string WeightUnitPreferenceKey = "settings_weight_unit";
    private const string MeasurementSystemPreferenceKey = "settings_measurement_system";
    private const string CloudSyncPreferenceKey = "settings_cloud_sync_enabled";

    private const string BodyWeightPreferenceKey = "settings_profile_body_weight";
    private const string HeightPreferenceKey = "settings_profile_height";
    private const string AgePreferenceKey = "settings_profile_age";
    private const string SexPreferenceKey = "settings_profile_sex";

    private const int MinAge = 13;
    private const int MaxAge = 120;

    [ObservableProperty]
    public partial string CurrentTheme { get; set; } = "Light";

    [ObservableProperty]
    public partial string WeightUnit { get; set; } = "kg";

    [ObservableProperty]
    public partial string MeasurementSystem { get; set; } = "Metric";

    [ObservableProperty]
    public partial bool IsCloudSyncEnabled { get; set; }

    [ObservableProperty]
    public partial string BodyWeightText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HeightText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AgeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sex { get; set; } = "Male";

    public bool IsLightTheme => CurrentTheme == "Light";
    public bool IsDarkTheme => CurrentTheme == "Dark";

    public bool IsKgSelected => WeightUnit == "kg";
    public bool IsLbsSelected => WeightUnit == "lbs";

    public bool IsMetricSelected => MeasurementSystem == "Metric";
    public bool IsImperialSelected => MeasurementSystem == "Imperial";

    public bool IsMaleSelected => Sex == "Male";
    public bool IsFemaleSelected => Sex == "Female";

    public string ThemeSummary => IsDarkTheme ? "Dark mode enabled" : "Light mode enabled";

    public string UnitsSummary =>
        $"{(IsKgSelected ? "Kilograms" : "Pounds")} • {(IsMetricSelected ? "Metric" : "Imperial")}";

    public string CloudSyncSummary => IsCloudSyncEnabled
        ? "Cloud sync is enabled locally. Remote sync flow is not implemented yet."
        : "Cloud sync is disabled.";

    public string UserProfileSummary
    {
        get
        {
            var weightText = string.IsNullOrWhiteSpace(BodyWeightText)
                ? "Weight not set"
                : $"{BodyWeightText} {(IsKgSelected ? "kg" : "lbs")}";

            var heightUnit = IsMetricSelected ? "cm" : "in";
            var heightText = string.IsNullOrWhiteSpace(HeightText)
                ? "height not set"
                : $"{HeightText} {heightUnit}";

            var ageText = string.IsNullOrWhiteSpace(AgeText)
                ? "age not set"
                : $"{AgeText} y";

            return $"{weightText} • {heightText} • {ageText} • {Sex}";
        }
    }

    public string UserProfileHint =>
        $"Used for BMI/BMR and calorie estimates. Age is limited to {MinAge}-{MaxAge}.";

    public SettingsPageViewModel(IThemeService themeService)
    {
        _themeService = themeService;

        Title = "Settings";
        CurrentTheme = _themeService.CurrentTheme;

        WeightUnit = Preferences.Default.Get(WeightUnitPreferenceKey, "kg");
        MeasurementSystem = Preferences.Default.Get(MeasurementSystemPreferenceKey, "Metric");
        IsCloudSyncEnabled = Preferences.Default.Get(CloudSyncPreferenceKey, false);

        BodyWeightText = Preferences.Default.Get(BodyWeightPreferenceKey, string.Empty);
        HeightText = Preferences.Default.Get(HeightPreferenceKey, string.Empty);

        var savedAge = Preferences.Default.Get(AgePreferenceKey, 0);
        AgeText = savedAge > 0 ? savedAge.ToString(CultureInfo.InvariantCulture) : string.Empty;

        Sex = Preferences.Default.Get(SexPreferenceKey, "Male");

        NormalizeState();
        RaiseComputedProperties();
    }

    partial void OnCurrentThemeChanged(string value)
    {
        CurrentTheme = value == "Dark" ? "Dark" : "Light";
        RaiseComputedProperties();
    }

    partial void OnWeightUnitChanged(string value)
    {
        WeightUnit = value == "lbs" ? "lbs" : "kg";
        Preferences.Default.Set(WeightUnitPreferenceKey, WeightUnit);
        RaiseComputedProperties();
    }

    partial void OnMeasurementSystemChanged(string value)
    {
        MeasurementSystem = value == "Imperial" ? "Imperial" : "Metric";
        Preferences.Default.Set(MeasurementSystemPreferenceKey, MeasurementSystem);
        RaiseComputedProperties();
    }

    partial void OnIsCloudSyncEnabledChanged(bool value)
    {
        Preferences.Default.Set(CloudSyncPreferenceKey, value);
        OnPropertyChanged(nameof(CloudSyncSummary));
    }

    partial void OnBodyWeightTextChanged(string value)
    {
        BodyWeightText = NormalizeDecimalText(value);
        Preferences.Default.Set(BodyWeightPreferenceKey, BodyWeightText);
        OnPropertyChanged(nameof(UserProfileSummary));
    }

    partial void OnHeightTextChanged(string value)
    {
        HeightText = NormalizeDecimalText(value);
        Preferences.Default.Set(HeightPreferenceKey, HeightText);
        OnPropertyChanged(nameof(UserProfileSummary));
    }

    partial void OnAgeTextChanged(string value)
    {
        AgeText = NormalizeAgeText(value);
        Preferences.Default.Set(AgePreferenceKey, ParseAgeOrDefault(AgeText));
        OnPropertyChanged(nameof(UserProfileSummary));
    }

    partial void OnSexChanged(string value)
    {
        Sex = value == "Female" ? "Female" : "Male";
        Preferences.Default.Set(SexPreferenceKey, Sex);
        RaiseComputedProperties();
    }

    [RelayCommand]
    private void ApplyLightTheme()
    {
        _themeService.SetTheme("Light");
        CurrentTheme = _themeService.CurrentTheme;
    }

    [RelayCommand]
    private void ApplyDarkTheme()
    {
        _themeService.SetTheme("Dark");
        CurrentTheme = _themeService.CurrentTheme;
    }

    [RelayCommand]
    private void SelectKg()
    {
        WeightUnit = "kg";
    }

    [RelayCommand]
    private void SelectLbs()
    {
        WeightUnit = "lbs";
    }

    [RelayCommand]
    private void SelectMetric()
    {
        MeasurementSystem = "Metric";
    }

    [RelayCommand]
    private void SelectImperial()
    {
        MeasurementSystem = "Imperial";
    }

    [RelayCommand]
    private void SelectMale()
    {
        Sex = "Male";
    }

    [RelayCommand]
    private void SelectFemale()
    {
        Sex = "Female";
    }

    private void NormalizeState()
    {
        CurrentTheme = CurrentTheme == "Dark" ? "Dark" : "Light";
        WeightUnit = WeightUnit == "lbs" ? "lbs" : "kg";
        MeasurementSystem = MeasurementSystem == "Imperial" ? "Imperial" : "Metric";
        Sex = Sex == "Female" ? "Female" : "Male";

        BodyWeightText = NormalizeDecimalText(BodyWeightText);
        HeightText = NormalizeDecimalText(HeightText);
        AgeText = NormalizeAgeText(AgeText);
    }

    private void RaiseComputedProperties()
    {
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(IsKgSelected));
        OnPropertyChanged(nameof(IsLbsSelected));
        OnPropertyChanged(nameof(IsMetricSelected));
        OnPropertyChanged(nameof(IsImperialSelected));
        OnPropertyChanged(nameof(IsMaleSelected));
        OnPropertyChanged(nameof(IsFemaleSelected));
        OnPropertyChanged(nameof(ThemeSummary));
        OnPropertyChanged(nameof(UnitsSummary));
        OnPropertyChanged(nameof(CloudSyncSummary));
        OnPropertyChanged(nameof(UserProfileSummary));
        OnPropertyChanged(nameof(UserProfileHint));
    }

    private static string NormalizeDecimalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().Replace(',', '.');

        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            if (parsed < 0)
                parsed = 0;

            return parsed.ToString("0.##", CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }

    private static string NormalizeAgeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            return string.Empty;

        parsed = Math.Clamp(parsed, MinAge, MaxAge);
        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static int ParseAgeOrDefault(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? Math.Clamp(parsed, MinAge, MaxAge)
            : 0;
    }
}