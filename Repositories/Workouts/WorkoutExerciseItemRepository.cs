using XerSize.Models.DataAccessObjects.Workouts;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.Workouts;

public sealed class WorkoutExerciseItemRepository
    : SqliteRepository<WorkoutExerciseItemModel, Guid>
{
    public WorkoutExerciseItemRepository(SqliteLocalStore database)
        : base(database, "WorkoutExercises")
    {
    }
}