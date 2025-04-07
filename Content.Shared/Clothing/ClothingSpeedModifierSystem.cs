// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing;

public sealed class ClothingSpeedModifierSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingSpeedModifierComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, ComponentHandleState>(OnHandleState);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, GetVerbsEvent<ExamineVerb>>(OnClothingVerbExamine);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnGetState(EntityUid uid, ClothingSpeedModifierComponent component, ref ComponentGetState args)
    {
        args.State = new ClothingSpeedModifierComponentState(component.WalkModifier, component.SprintModifier);
    }

    private void OnHandleState(EntityUid uid, ClothingSpeedModifierComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ClothingSpeedModifierComponentState state)
            return;

        var diff = !MathHelper.CloseTo(component.SprintModifier, state.SprintModifier) ||
                   !MathHelper.CloseTo(component.WalkModifier, state.WalkModifier);

        component.WalkModifier = state.WalkModifier;
        component.SprintModifier = state.SprintModifier;

        // Avoid raising the event for the container if nothing changed.
        // We'll still set the values in case they're slightly different but within tolerance.
        if (diff && _container.TryGetContainingContainer((uid, null, null), out var container))
        {
            _movementSpeed.RefreshMovementSpeedModifiers(container.Owner);
        }
    }

    private void OnRefreshMoveSpeed(EntityUid uid, ClothingSpeedModifierComponent component, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (_toggle.IsActivated(uid))
            args.Args.ModifySpeed(component.WalkModifier, component.SprintModifier);
    }

    private void OnClothingVerbExamine(EntityUid uid, ClothingSpeedModifierComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var walkModifierPercentage = MathF.Round((1.0f - component.WalkModifier) * 100f, 1);
        var sprintModifierPercentage = MathF.Round((1.0f - component.SprintModifier) * 100f, 1);

        if (walkModifierPercentage == 0.0f && sprintModifierPercentage == 0.0f)
            return;

        var msg = new FormattedMessage();

        if (MathHelper.CloseTo(walkModifierPercentage, sprintModifierPercentage, 0.5f))
        {
            if (walkModifierPercentage < 0.0f)
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-increase-equal-examine", ("walkSpeed", (int) MathF.Abs(walkModifierPercentage)), ("runSpeed", (int) MathF.Abs(sprintModifierPercentage))));
            else
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-decrease-equal-examine", ("walkSpeed", (int) walkModifierPercentage), ("runSpeed", (int) sprintModifierPercentage)));
        }
        else
        {
            if (sprintModifierPercentage < 0.0f)
            {
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-increase-run-examine", ("runSpeed", (int) MathF.Abs(sprintModifierPercentage))));
            }
            else if (sprintModifierPercentage > 0.0f)
            {
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-decrease-run-examine", ("runSpeed", (int) sprintModifierPercentage)));
            }
            if (walkModifierPercentage != 0.0f && sprintModifierPercentage != 0.0f)
            {
                msg.PushNewline();
            }
            if (walkModifierPercentage < 0.0f)
            {
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-increase-walk-examine", ("walkSpeed", (int) MathF.Abs(walkModifierPercentage))));
            }
            else if (walkModifierPercentage > 0.0f)
            {
                msg.AddMarkupOrThrow(Loc.GetString("clothing-speed-decrease-walk-examine", ("walkSpeed", (int) walkModifierPercentage)));
            }
        }

        _examine.AddDetailedExamineVerb(args, component, msg, Loc.GetString("clothing-speed-examinable-verb-text"), "/Textures/Interface/VerbIcons/outfit.svg.192dpi.png", Loc.GetString("clothing-speed-examinable-verb-message"));
    }

    private void OnToggled(Entity<ClothingSpeedModifierComponent> ent, ref ItemToggledEvent args)
    {
        // make sentient boots slow or fast too
        _movementSpeed.RefreshMovementSpeedModifiers(ent);

        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container))
        {
            // inventory system will automatically hook into the event raised by this and update accordingly
            _movementSpeed.RefreshMovementSpeedModifiers(container.Owner);
        }
    }
}
