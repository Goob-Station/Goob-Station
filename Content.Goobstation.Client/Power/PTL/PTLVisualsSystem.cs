// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Power.PTL;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Power.PTL;

public sealed partial class PTLVisualsSystem : VisualizerSystem<PTLVisualsComponent>
{
    [Dependency] private readonly IGameTiming _time = default!;

    protected override void OnAppearanceChange(EntityUid uid, PTLVisualsComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (!TryComp<SpriteComponent>(uid, out var sprite)
        || !TryComp<PTLComponent>(uid, out var ptl))
            return;

        sprite.LayerSetVisible(PTLVisualLayers.Unpowered, !ptl.Active);

        var delta = (ptl.NextShotAt - _time.CurTime).Seconds;
        var norm = (delta / ptl.ShootDelay) * component.MaxChargeStates;
        sprite.LayerSetState(PTLVisualLayers.Charge, $"{component.ChargePrefix}{(int) norm}");
    }
}

enum PTLVisualLayers : byte
{
    Base,
    Unpowered,
    Charge
}
