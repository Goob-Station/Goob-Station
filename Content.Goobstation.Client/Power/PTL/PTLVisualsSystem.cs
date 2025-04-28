// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Power.PTL;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Power.PTL;

public sealed partial class PTLVisualsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _time = default!;

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

        sprite.LayerSetVisible(PTLVisualLayers.Unpowered, ptl.Active);

        var delta = (ptl.NextShotAt - _time.CurTime).Seconds;
        var norm = Math.Clamp(delta / ptl.ShootDelay * ent.Comp.MaxChargeStates, 1, ent.Comp.MaxChargeStates);
        sprite.LayerSetState(PTLVisualLayers.Charge, $"{ent.Comp.ChargePrefix}{(int) norm}");
    }
}

enum PTLVisualLayers : byte
{
    Base,
    Unpowered,
    Charge
}
