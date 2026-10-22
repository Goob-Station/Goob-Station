using System.Threading;
using System.Threading.Tasks;
using Robust.Shared.Network;

namespace Content.Server.Database;

public partial interface IServerDbManager
{
    Task<List<AchievementProgress>> GetAchievementProgressAsync(NetUserId userId, CancellationToken cancel = default);

    Task AddAchievementProgressAsync(NetUserId userId, string achievementId, int delta, DateTime? unlockedAt);

    Task<(int Players, Dictionary<string, int> Unlocks)> GetAchievementStatsAsync(CancellationToken cancel = default);
}

public sealed partial class ServerDbManager
{
    public Task<List<AchievementProgress>> GetAchievementProgressAsync(NetUserId userId, CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetAchievementProgressAsync(userId, cancel));
    }

    public Task AddAchievementProgressAsync(NetUserId userId, string achievementId, int delta, DateTime? unlockedAt)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.AddAchievementProgressAsync(userId, achievementId, delta, unlockedAt));
    }

    public Task<(int Players, Dictionary<string, int> Unlocks)> GetAchievementStatsAsync(CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetAchievementStatsAsync(cancel));
    }
}
