using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Network;

namespace Content.Server.Database;

public abstract partial class ServerDbBase
{
    public async Task<List<AchievementProgress>> GetAchievementProgressAsync(NetUserId userId, CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);

        var list = await db.DbContext.AchievementProgress
            .AsNoTracking()
            .Where(a => a.PlayerUserId == userId.UserId)
            .ToListAsync(cancel);

        foreach (var entry in list)
        {
            entry.UnlockedAt = NormalizeDatabaseTime(entry.UnlockedAt);
        }

        return list;
    }

    public async Task AddAchievementProgressAsync(NetUserId userId, string achievementId, int delta, DateTime? unlockedAt)
    {
        await using var db = await GetDb();

        if (await TryIncrementAchievement(db.DbContext, userId.UserId, achievementId, delta, unlockedAt))
            return;

        db.DbContext.AchievementProgress.Add(new AchievementProgress
        {
            PlayerUserId = userId.UserId,
            AchievementId = achievementId,
            Progress = delta,
            UnlockedAt = unlockedAt,
        });

        try
        {
            await db.DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.DbContext.ChangeTracker.Clear();
            await TryIncrementAchievement(db.DbContext, userId.UserId, achievementId, delta, unlockedAt);
        }
    }

    private static async Task<bool> TryIncrementAchievement(
        ServerDbContext context,
        Guid userId,
        string achievementId,
        int delta,
        DateTime? unlockedAt)
    {
        var updated = await context.AchievementProgress
            .Where(a => a.PlayerUserId == userId && a.AchievementId == achievementId)
            .ExecuteUpdateAsync(set => set
                .SetProperty(a => a.Progress, a => a.Progress + delta)
                .SetProperty(a => a.UnlockedAt, a => a.UnlockedAt ?? unlockedAt));

        return updated > 0;
    }

    public async Task<(int Players, Dictionary<string, int> Unlocks)> GetAchievementStatsAsync(CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);

        var players = await db.DbContext.AchievementProgress
            .Select(a => a.PlayerUserId)
            .Distinct()
            .CountAsync(cancel);

        var unlocks = await db.DbContext.AchievementProgress
            .Where(a => a.UnlockedAt != null)
            .GroupBy(a => a.AchievementId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count, cancel);

        return (players, unlocks);
    }
}
