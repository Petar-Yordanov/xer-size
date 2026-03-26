using CommunityToolkit.Mvvm.ComponentModel;
using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.ViewModels.History;

[QueryProperty(nameof(SessionIdText), "sessionId")]
public partial class HistorySessionDetailsPageViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;

    [ObservableProperty]
    public partial string SessionIdText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial HistorySessionDetails? Session { get; set; }

    public HistorySessionDetailsPageViewModel(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
    }

    partial void OnSessionIdTextChanged(string value)
    {
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        if (!Guid.TryParse(SessionIdText, out var sessionId))
            return;

        var allSessions = await _historyService.GetAllAsync();
        var session = allSessions.FirstOrDefault(x => x.Id == sessionId);

        if (session is null)
            return;

        Session = new HistorySessionDetails
        {
            SessionId = session.Id,
            RoutineName = session.RoutineName,
            WorkoutName = session.WorkoutName,
            PerformedAt = session.PerformedAt,
            DurationMinutes = session.DurationMinutes,
            EstimatedCaloriesBurned = session.EstimatedCaloriesBurned,
            BodyWeightKg = session.BodyWeightKg,
            TotalVolume = session.TotalVolume,
            TotalSets = session.TotalSets,
            TotalReps = session.Exercises.Sum(e => e.Sets.Sum(s => s.Reps ?? 0)),
            Exercises = session.Exercises
                .OrderBy(x => x.Name)
                .Select(e => new HistoryDetailExerciseRow
                {
                    Name = e.Name,
                    MetaText = $"{e.Equipment} • {e.Mechanic} • {e.BodyCategory}",
                    VolumeText = $"Volume: {e.TotalVolume:F0} kg • Sets: {e.Sets.Count} • Reps: {e.Sets.Sum(s => s.Reps ?? 0)}",
                    Sets = e.Sets
                        .OrderBy(s => s.Order)
                        .Select(s => new HistoryDetailSetRow
                        {
                            Text = BuildSetText(s)
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static string BuildSetText(LoggedSet set)
    {
        if (set.DurationSeconds.HasValue && set.DurationSeconds.Value > 0)
        {
            return $"Set {set.Order}: {set.DurationSeconds.Value}s"
                 + (set.RestSeconds.HasValue ? $" • Rest {set.RestSeconds.Value}s" : string.Empty);
        }

        return $"Set {set.Order}: {set.Reps ?? 0} reps x {(set.WeightKg ?? 0):F1} kg"
             + (set.RestSeconds.HasValue ? $" • Rest {set.RestSeconds.Value}s" : string.Empty);
    }
}