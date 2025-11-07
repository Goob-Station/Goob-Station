using Content.Server.Destructible;
using Content.Shared._Shitcode.Heretic.Systems;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class RustChargeSystem : SharedRustChargeSystem
{
    [Dependency] private readonly DestructibleSystem _destructible = default!;

    protected override void DestroyStructure(EntityUid uid, EntityUid user)
    {
        base.DestroyStructure(uid, user);

        if (!TryComp(uid, out DestructibleComponent? destructible) || destructible.Thresholds.Count == 0)
        {
            Del(uid);
            return;
        }

        var threshold = destructible.Thresholds[^1];
        RaiseLocalEvent(uid, new DamageThresholdReached(destructible, threshold), true);
        threshold.Execute(uid, _destructible, EntityManager, user);
    }
}
