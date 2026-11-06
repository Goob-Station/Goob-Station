using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Random;

/// <summary>
/// Helper system for <see cref="SharedRandomExtensions"/> because slop repeated code
/// </summary>
public sealed partial class PredictedRandomSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public IRobustRandom Random(NetEntity netEnt1, NetEntity? netEnt2)
    {
        return SharedRandomExtensions.PredictedRandom(_timing, netEnt1, netEnt2);
    }

    public IRobustRandom Random(EntityUid uid1, EntityUid? uid2)
    {
        return Random(GetNetEntity(uid1), GetNetEntity(uid2));
    }

    public bool Prob(NetEntity netEnt1, NetEntity? netEnt2, float prob)
    {
        return Random(netEnt1, netEnt2).Prob(prob);
    }

    public bool Prob(EntityUid uid1, EntityUid? uid2, float prob)
    {
        return Prob(GetNetEntity(uid1), GetNetEntity(uid2), prob);
    }

    public T Pick<T>(NetEntity netEnt1, NetEntity? netEnt2, IReadOnlyList<T> list)
    {
        return Random(netEnt1, netEnt2).Pick(list);
    }

    public T Pick<T>(EntityUid uid1, EntityUid? uid2, IReadOnlyList<T> list)
    {
        return Pick(GetNetEntity(uid1), GetNetEntity(uid2), list);
    }
}
