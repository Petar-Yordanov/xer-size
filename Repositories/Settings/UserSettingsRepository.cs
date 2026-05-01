using XerSize.Models.DataAccessObjects.Settings;
using XerSize.Repositories.Common;

namespace XerSize.Repositories.Settings;

public sealed class UserSettingsRepository
    : SqliteRepository<UserSettingsModel, Guid>
{
    public UserSettingsRepository(SqliteLocalStore database)
        : base(database, "UserSettings")
    {
    }
}