// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodCultPhaseShiftSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultPhaseShiftActionEvent>(OnPhaseShift);
    }

    private void OnPhaseShift(BloodCultPhaseShiftActionEvent args)
    {
        if (args.Handled || !HasComp<BloodCultConstructComponent>(args.Performer))
            return;

        var phaseShift = new PhaseShiftedComponent
        {
            CollisionMask = (int) CollisionGroup.GhostImpassable,
            CollisionLayer = (int) CollisionGroup.None,
            PhaseInEffect = "BloodCultTeleportInEffect",
            PhaseOutEffect = "BloodCultTeleportOutEffect",
            PhaseInSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/wraith_phase.ogg"),
            PhaseOutSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/wraith_phase.ogg"),
        };

#pragma warning disable CS0618 // Pirate: the existing Goob phase-shift component still uses the legacy status API.
        if (!_statusEffects.TryAddStatusEffect(
                args.Performer,
                "PhaseShifted",
                args.Duration,
                true,
                phaseShift))
            return;
#pragma warning restore CS0618

        args.Handled = true;
    }
}
