using Content.Goobstation.Shared.Voidwalker.Spaced;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Voidwalker.Spaced;

public sealed partial class SpacedStatusSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpacedStatusComponent, CheckSpacedStatusEvent>(OnCheckTileSpacedStatus);
    }

    /// <inheritdoc />
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<SpacedStatusComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.CheckOnInterval || curTime <= comp.NextSpacedCheck)
                continue;

            UpdateSpacedStatus((uid, comp));
            comp.NextSpacedCheck = curTime + comp.SpacedCheckInterval;
        }
    }

    public void OnCheckTileSpacedStatus(Entity<SpacedStatusComponent> entity, ref CheckSpacedStatusEvent args)
    {
        (args.Spaced, args.Changed) = UpdateSpacedStatus(entity);
    }

    public (bool, bool) UpdateSpacedStatus(Entity<SpacedStatusComponent> entity)
    {
        var spaced = IsCreatureSpaced(entity);
        var changed = spaced != entity.Comp.IsInSpace;
        entity.Comp.Changed = changed;

        entity.Comp.IsInSpace = spaced;
        Dirty(entity);
        return (spaced, changed);
    }

    public bool IsCreatureSpaced(EntityUid entity)
    {
        var gas = _atmos.GetContainingMixture(entity);
        return gas is not { Pressure: > 0 };
    }
}
