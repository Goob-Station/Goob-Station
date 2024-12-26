using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Antag;

public sealed class AntagSelectionPlayerPool (List<List<ICommonSession>> orderedPools)
{
    public bool TryPickAndTake(IRobustRandom random, [NotNullWhen(true)] out ICommonSession? session)
    {
        session = null;

        foreach (var pool in orderedPools)
        {
            if (pool.Count == 0)
                continue;

            session = random.PickAndTake(pool);
            break;
        }

        return session != null;
    }

    // Einstein Engines change
    public bool TryGetItems(IRobustRandom random,
                            [NotNullWhen(true)] out ICommonSession[]? sessions,
                            int count,
                            bool allowDuplicates = true)
    {
        DebugTools.Assert(count > 0, $"The count {nameof(count)} of requested sessions must be greater than zero!");

        sessions = null;
        List<ICommonSession> session_list = [];

        foreach (var pool in orderedPools)
        {
            if (pool.Count == 0)
                continue;

            var picked = random.GetItems(pool, count - session_list.Count, allowDuplicates);
            session_list.AddRange(picked);
            if (session_list.Count < count)
            {
                continue;
            }
            sessions = session_list.ToArray();
            break;
        }

        return sessions != null;
    }

    // Einstein Engines change
    public AntagSelectionPlayerPool Where(Func<ICommonSession, bool> predicate)
    {
        var newPools = orderedPools.Select(
            (pool) =>
            {
                return pool.Where(predicate).ToList();
            }
        );

        return new AntagSelectionPlayerPool(newPools.ToList());
    }

    public int Count => orderedPools.Sum(p => p.Count);
}
