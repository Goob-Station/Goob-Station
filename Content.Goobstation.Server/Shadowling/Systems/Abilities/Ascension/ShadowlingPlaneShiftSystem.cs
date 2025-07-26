// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PhaseShift;
using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Plane Shift ability.
/// A toogleable ability that lets you phase through walls!
/// </summary>
public sealed class ShadowlingPlaneShiftSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingPlaneShiftComponent, TogglePlaneShiftEvent>(OnPlaneShift);
    }

    private void OnPlaneShift(EntityUid uid, ShadowlingPlaneShiftComponent comp, TogglePlaneShiftEvent args)
    {
        comp.IsActive = !comp.IsActive;
        if (comp.IsActive)
        {
            TryDoShift(uid);
        }
        else
        {
            if (!HasComp<PhaseShiftedComponent>(uid))
                return;

            RemComp<PhaseShiftedComponent>(uid);
        }

        _actions.StartUseDelay(args.Action);
    }

    private void TryDoShift(EntityUid uid)
    {
        if (HasComp<PhaseShiftedComponent>(uid))
            return;

        var phaseShift = EnsureComp<PhaseShiftedComponent>(uid);
        phaseShift.MovementSpeedBuff = 1.7f;
        // Thanks to blood cult code for this component
    }
}
