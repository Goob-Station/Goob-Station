// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Traits.Components;
using Content.Shared.Examine;
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
        {
            args.PushMarkup(Loc.GetString("movement-impaired-trait-examined", ("target", Identity.Entity(comp, EntityManager))));
        }
    }

    private void OnItemEquip(EntityUid uid, MovementImpairedComponent comp, DidEquipHandEvent args)
    {
        if (!TryComp<MovementImpairedCorrectionComponent>(args.Equipped, out var correctionComp))
            return;

        if (correctionComp.SpeedCorrection == 0)
        {
            comp.CorrectionCounter++; // Track how many "full correction" items are equipped
            if (comp.CorrectionCounter == 1)
            {
                comp.BaseImpairedSpeedMultiplier = comp.ImpairedSpeedMultiplier;
                comp.ImpairedSpeedMultiplier = 1;
            }
        }
        else
        {
            var multiplier = Math.Clamp(comp.ImpairedSpeedMultiplier + correctionComp.SpeedCorrection, 0f, 1f);
            comp.ImpairedSpeedMultiplier = multiplier;
        }

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnItemUnequip(EntityUid uid, MovementImpairedComponent comp, DidUnequipHandEvent args)
    {
        if (!TryComp<MovementImpairedCorrectionComponent>(args.Unequipped, out var correctionComp))
            return;

        if (correctionComp.SpeedCorrection == 0)
        {
            // Only reset speed if this is the last full correction being dropped.
            if (comp.CorrectionCounter == 1)
                comp.ImpairedSpeedMultiplier = comp.BaseImpairedSpeedMultiplier;

            // Decrement counter
            comp.CorrectionCounter--;
        }
        else
        {
            var multiplier = Math.Clamp(comp.ImpairedSpeedMultiplier - correctionComp.SpeedCorrection, 0f, 1f);
            comp.ImpairedSpeedMultiplier = multiplier;

            // If you somehow manage to reach zero speed, reset to the base speed.
            if (comp.ImpairedSpeedMultiplier == 0)
                comp.ImpairedSpeedMultiplier = comp.BaseImpairedSpeedMultiplier;
            // This lags a bit, but you usually shouldn't see this unless something is borked.
        }

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnModifierRefresh(EntityUid uid, MovementImpairedComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(comp.ImpairedSpeedMultiplier);
        Dirty(uid, comp);
    }
}
