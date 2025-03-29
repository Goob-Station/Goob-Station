using System.Linq;
using Content.Goobstation.Common.Standing;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Map;

namespace Content.Shared._White.Standing;

public sealed class TelefragSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public void DoTelefrag(EntityUid uid,
        EntityCoordinates coords,
        TimeSpan knockdownTime,
        float range = 0.3f,
        DropHeldItemsBehavior behavior = DropHeldItemsBehavior.NoDrop,
        bool autoStandUp = false)
    {
        if (range <= 0f)
            return;

        var entities = _lookup.GetEntitiesInRange(coords, range, LookupFlags.Dynamic);
        foreach (var ent in entities.Where(ent => ent != uid && !_standing.IsDown(ent)))
        {
            if (knockdownTime > TimeSpan.Zero && _stun.TryKnockdown(ent, knockdownTime, true, behavior))
                continue;

            if (_layingDown.TryLieDown(ent, behavior: behavior) && autoStandUp)
                _layingDown.TryStandUp(ent);
        }
    }
}
