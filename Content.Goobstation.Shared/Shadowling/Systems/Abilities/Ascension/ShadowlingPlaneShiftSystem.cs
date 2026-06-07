// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PhaseShift;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Plane Shift ability.
/// A toogleable ability that lets you phase through walls!
/// </summary>
public sealed class ShadowlingPlaneShiftSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingPlaneShiftComponent, TogglePlaneShiftEvent>(OnPlaneShift);
        SubscribeLocalEvent<ShadowlingPlaneShiftComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingPlaneShiftComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingPlaneShiftComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingPlaneShiftComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnPlaneShift(EntityUid uid, ShadowlingPlaneShiftComponent comp, TogglePlaneShiftEvent args)
    {
        if (args.Handled)
            return;

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

        args.Handled = true;
    }

    private void TryDoShift(EntityUid uid)
    {
        var phaseShift = EnsureComp<PhaseShiftedComponent>(uid);
        phaseShift.MovementSpeedBuff = 1.7f;
    }
}
