using Content.Shared._Lavaland.Weapons.Block;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Weapons.Block;

public sealed class BlockChargeSystem : SharedBlockChargeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BlockChargeComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.HasCharge || curTime < comp.NextCharge)
                continue;

            comp.HasCharge = true;
            if (Transform(uid).ParentUid is { } parent
                && HasComp<BlockChargeUserComponent>(parent)) // Just to know if its a valid entity holding it.
                _popup.PopupEntity(Loc.GetString("block-charge-startup", ("entity", uid)), parent, parent);

            Dirty(uid, comp);
        }
    }
}
