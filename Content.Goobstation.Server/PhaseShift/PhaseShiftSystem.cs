// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.Eye;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.PhaseShift;

public sealed class PhaseShiftSystem : SharedPhaseShiftSystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;

    protected override void OnComponentStartup(Entity<PhaseShiftedComponent> ent, ref ComponentStartup args)
    {
        base.OnComponentStartup(ent, ref args);

        if (!TryComp<VisibilityComponent>(ent, out var visibility))
            return;

        _visibilitySystem.AddLayer((ent, visibility), (int) VisibilityFlags.Ghost, false);
        _visibilitySystem.RemoveLayer((ent, visibility), (int) VisibilityFlags.Normal, false);
        _visibilitySystem.RefreshVisibility(ent);
    }

    protected override void OnComponentShutdown(Entity<PhaseShiftedComponent> ent, ref ComponentShutdown args)
    {
        base.OnComponentShutdown(ent, ref args);

        if (!TryComp<VisibilityComponent>(ent, out var visibility))
            return;

        _visibilitySystem.RemoveLayer((ent, visibility), (int) VisibilityFlags.Ghost, false);
        _visibilitySystem.AddLayer((ent, visibility), (int) VisibilityFlags.Normal, false);
        _visibilitySystem.RefreshVisibility(ent);
    }
}
