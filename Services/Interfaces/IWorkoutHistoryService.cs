using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IWorkoutHistoryService
{
    Task<IReadOnlyList<LoggedWorkoutSession>> GetAllAsync();
    Task AddSessionAsync(LoggedWorkoutSession session);
}