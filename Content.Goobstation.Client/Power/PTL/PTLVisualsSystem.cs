// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Power.PTL;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Power.PTL;

public sealed partial class PTLVisualsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<PTLVisualsComponent>();
        while (eqe.MoveNext(out var uid, out var ptlv))
            UpdateVisuals((uid, ptlv));
    }

    private void UpdateVisuals(Entity<PTLVisualsComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)
        || !TryComp<PTLComponent>(ent, out var ptl))
            return;

        _sprite.LayerSetVisible((ent, sprite), PTLVisualLayers.Base, true);

        var delta = (ptl.NextShotAt - _time.CurTime).Seconds;
        var norm = Math.Clamp(delta / ptl.ShootDelay * ent.Comp.MaxChargeStates, 1, ent.Comp.MaxChargeStates);
        _sprite.LayerSetRsiState((ent, sprite), (int) norm, ent.Comp.ChargePrefix);
    }
}

enum PTLVisualLayers : byte
{
    Base,
    Unpowered,
    Charge
}
