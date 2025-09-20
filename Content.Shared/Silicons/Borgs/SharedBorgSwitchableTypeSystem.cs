// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Borgs;

/// <summary>
/// Implements borg type switching.
/// </summary>
/// <seealso cref="BorgSwitchableTypeComponent"/>
public abstract class SharedBorgSwitchableTypeSystem : EntitySystem
{
    // TODO: Allow borgs to be reset to default configuration.

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] protected readonly IPrototypeManager Prototypes = default!;
    [Dependency] private readonly InteractionPopupSystem _interactionPopup = default!;

    public static readonly EntProtoId ActionId = "ActionSelectBorgType";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableTypeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BorgSwitchableTypeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BorgSwitchableTypeComponent, BorgToggleSelectTypeEvent>(OnSelectBorgTypeAction);

        Subs.BuiEvents<BorgSwitchableTypeComponent>(BorgSwitchableTypeUiKey.SelectBorgType,
            sub =>
            {
                sub.Event<BorgSelectTypeMessage>(SelectTypeMessageHandler);
            });
    }

    //
    // UI-adjacent code
    //

    private void OnMapInit(Entity<BorgSwitchableTypeComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.SelectTypeAction, ActionId);
        Dirty(ent);

        if (ent.Comp.SelectedBorgType != null &&
            TryComp(ent, out BorgSwitchableSubtypeComponent? subtype) &&
            subtype.BorgSubtype != null)
        {
            SelectBorgModule(ent, ent.Comp.SelectedBorgType.Value, subtype.BorgSubtype.Value);
        }
    }

    private void OnShutdown(Entity<BorgSwitchableTypeComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.SelectTypeAction);
    }

    private void OnSelectBorgTypeAction(Entity<BorgSwitchableTypeComponent> ent, ref BorgToggleSelectTypeEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(ent, out var actor))
            return;

        args.Handled = true;

        _userInterface.TryToggleUi((ent.Owner, null), BorgSwitchableTypeUiKey.SelectBorgType, actor.PlayerSession);
    }

    private void SelectTypeMessageHandler(Entity<BorgSwitchableTypeComponent> ent, ref BorgSelectTypeMessage args)
    {
        if (ent.Comp.SelectedBorgType != null)
            return;

        if (!Prototypes.HasIndex(args.Prototype) || !Prototypes.HasIndex(args.Subtype))
            return;

        SelectBorgModule(ent, args.Prototype, args.Subtype);
    }

    //
    // Implementation
    //

    protected virtual void SelectBorgModule(
        Entity<BorgSwitchableTypeComponent> ent,
        ProtoId<BorgTypePrototype> borgType,
        ProtoId<BorgSubtypePrototype> borgSubtype)
    {
        ent.Comp.SelectedBorgType = borgType;
        if (TryComp(ent, out BorgSwitchableSubtypeComponent? subtype))
            subtype.BorgSubtype = borgSubtype;

        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.SelectTypeAction);
        _userInterface.CloseUi(ent.Owner, BorgSwitchableTypeUiKey.SelectBorgType);
        ent.Comp.SelectTypeAction = null;
        Dirty(ent);
        if (subtype != null)
            Dirty(ent.Owner, subtype);

        UpdateEntityAppearance(ent);
    }

    protected void UpdateEntityAppearance(Entity<BorgSwitchableTypeComponent> entity)
    {
        if (!Prototypes.TryIndex(entity.Comp.SelectedBorgType, out var proto) ||
            !TryComp(entity, out BorgSwitchableSubtypeComponent? subtype) ||
            !Prototypes.TryIndex(subtype.BorgSubtype, out var subtypeProto))
            return;

        UpdateEntityAppearance(entity, proto, subtypeProto);
    }

    protected virtual void UpdateEntityAppearance(
        Entity<BorgSwitchableTypeComponent> entity,
        BorgTypePrototype prototype,
        BorgSubtypePrototype subtypePrototype)
    {
        if (TryComp(entity, out InteractionPopupComponent? popup))
        {
            _interactionPopup.SetInteractSuccessString((entity.Owner, popup), prototype.PetSuccessString);
            _interactionPopup.SetInteractFailureString((entity.Owner, popup), prototype.PetFailureString);
        }

        if (TryComp(entity, out FootstepModifierComponent? footstepModifier))
        {
            footstepModifier.FootstepSoundCollection = prototype.FootstepCollection;
        }
    }
}
