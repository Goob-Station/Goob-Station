// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Traits.Components;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Traits.Systems;

public sealed partial class MovementImpairedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MovementImpairedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MovementImpairedComponent, DidEquipHandEvent>(OnItemEquip);
        SubscribeLocalEvent<MovementImpairedComponent, DidUnequipHandEvent>(OnItemUnequip);
        SubscribeLocalEvent<MovementImpairedComponent, RefreshMovementSpeedModifiersEvent>(OnModifierRefresh);
        SubscribeLocalEvent<MovementImpairedComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMapInit(EntityUid uid, MovementImpairedComponent comp, MapInitEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnExamined(Entity<MovementImpairedComponent> comp, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient)
            args.PushMarkup(Loc.GetString("movement-impaired-trait-examined", ("target", Identity.Entity(comp, EntityManager))));
    }

    private void OnItemEquip(EntityUid uid, MovementImpairedComponent comp, DidEquipHandEvent args)
    {
        if (!TryComp<MovementImpairedCorrectionComponent>(args.Equipped, out var correctionComp))
            return;

        if (correctionComp.SpeedCorrection == 0)
        {
            comp.CorrectionCounter++;
            if (comp.CorrectionCounter == 1)
            {
                comp.BaseImpairedSpeedMultiplier = comp.ImpairedSpeedMultiplier;
                comp.ImpairedSpeedMultiplier = 1;
            }
        }
        else
        {
            var baseMultiplier = comp.ImpairedSpeedMultiplier + correctionComp.SpeedCorrection;
            if (baseMultiplier > 1)
                comp.SpeedCorrectionOverflow[args.Equipped] = baseMultiplier - 1;

            var totalOverflow = comp.SpeedCorrectionOverflow.Values.Aggregate((FixedPoint2)0, (a,b) => a + b);
            comp.ImpairedSpeedMultiplier = Math.Clamp((baseMultiplier + totalOverflow).Float(), 0, 1);
        }

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnItemUnequip(EntityUid uid, MovementImpairedComponent comp, ref DidUnequipHandEvent args)
    {
        if (!TryComp<MovementImpairedCorrectionComponent>(args.Unequipped, out var correctionComp))
            return;

        if (correctionComp.SpeedCorrection == 0)
        {
            comp.CorrectionCounter--;

            // Reset speed when all full corrections are removed
            if (comp.CorrectionCounter == 0)
                comp.ImpairedSpeedMultiplier = comp.BaseImpairedSpeedMultiplier;

            // Ensure CorrectionCounter doesn't go negative
            comp.CorrectionCounter = Math.Max(comp.CorrectionCounter, 0);
        }
        else
        {
            comp.SpeedCorrectionOverflow.TryGetValue(args.Unequipped, out var overflow);

            var baseMultiplier = comp.ImpairedSpeedMultiplier - correctionComp.SpeedCorrection + overflow;
            comp.ImpairedSpeedMultiplier = Math.Clamp(baseMultiplier.Float(), 0f, 1f);

            comp.SpeedCorrectionOverflow.Remove(args.Unequipped);
        }

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnModifierRefresh(EntityUid uid, MovementImpairedComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(comp.ImpairedSpeedMultiplier.Float());
        Dirty(uid, comp);
    }
}
