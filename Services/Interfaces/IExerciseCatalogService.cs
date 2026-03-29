using XerSize.Models;

namespace XerSize.Services.Interfaces;

public interface IExerciseCatalogService
{
    Task<IReadOnlyList<ExerciseCatalogItem>> GetAllAsync();
    Task<ExerciseCatalogItem?> GetByIdAsync(string id);
}