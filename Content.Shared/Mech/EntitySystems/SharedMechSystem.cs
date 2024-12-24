using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

// Goobstation Change
using Content.Shared.CCVar;
using Content.Shared._Goobstation.CCVar;
using Content.Shared.Emag.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.Configuration;
using Content.Shared.Implants.Components;

namespace Content.Shared.Mech.EntitySystems;

/// <summary>
/// Handles all of the interactions, UI handling, and items shennanigans for <see cref="MechComponent"/>
/// </summary>
public abstract class SharedMechSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!; // Goobstation Change
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!; // Goobstation Change
    [Dependency] private readonly IConfigurationManager _config = default!; // Goobstation Change

    // Goobstation: Local variable for checking if mech guns can be used out of them.
    private bool _canUseMechGunOutside;
    
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MechComponent, MechToggleEquipmentEvent>(OnToggleEquipmentAction);
        SubscribeLocalEvent<MechComponent, MechEjectPilotEvent>(OnEjectPilotEvent);
        SubscribeLocalEvent<MechComponent, UserActivateInWorldEvent>(RelayInteractionEvent);
        SubscribeLocalEvent<MechComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MechComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MechComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
        SubscribeLocalEvent<MechComponent, DragDropTargetEvent>(OnDragDrop);
        SubscribeLocalEvent<MechComponent, CanDropTargetEvent>(OnCanDragDrop);
        SubscribeLocalEvent<MechComponent, GotEmaggedEvent>(OnEmagged);

        SubscribeLocalEvent<MechPilotComponent, GetMeleeWeaponEvent>(OnGetMeleeWeapon);
        SubscribeLocalEvent<MechPilotComponent, CanAttackFromContainerEvent>(OnCanAttackFromContainer);
        SubscribeLocalEvent<MechPilotComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<MechPilotComponent, EntGotRemovedFromContainerMessage>(OnEntGotRemovedFromContainer);
        SubscribeLocalEvent<MechEquipmentComponent, ShotAttemptedEvent>(OnShotAttempted); // Goobstation
        Subs.CVar(_config, GoobCVars.MechGunOutsideMech, value => _canUseMechGunOutside = value, true); // Goobstation
    }

    // GoobStation: Fixes scram implants or teleports locking the pilot out of being able to move.
    private void OnEntGotRemovedFromContainer(EntityUid uid, MechPilotComponent component, EntGotRemovedFromContainerMessage args)
    {
        TryEject(component.Mech, pilot: uid);
    }

    private void OnToggleEquipmentAction(EntityUid uid, MechComponent component, MechToggleEquipmentEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;
        CycleEquipment(uid);
    }

    private void OnEjectPilotEvent(EntityUid uid, MechComponent component, MechEjectPilotEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;
        TryEject(uid, component);
    }

    private void RelayInteractionEvent(EntityUid uid, MechComponent component, UserActivateInWorldEvent args)
    {
        var pilot = component.PilotSlot.ContainedEntity;
        if (pilot == null)
            return;

        // TODO why is this being blocked?
        if (!_timing.IsFirstTimePredicted)
            return;

        if (component.CurrentSelectedEquipment != null)
        {
            RaiseLocalEvent(component.CurrentSelectedEquipment.Value, args);
        }
    }

    private void OnStartup(EntityUid uid, MechComponent component, ComponentStartup args)
    {
        component.PilotSlot = _container.EnsureContainer<ContainerSlot>(uid, component.PilotSlotId);
        component.EquipmentContainer = _container.EnsureContainer<Container>(uid, component.EquipmentContainerId);
        component.BatterySlot = _container.EnsureContainer<ContainerSlot>(uid, component.BatterySlotId);
        UpdateAppearance(uid, component);
    }

    private void OnDestruction(EntityUid uid, MechComponent component, DestructionEventArgs args)
    {
        BreakMech(uid, component);
    }

    private void OnGetAdditionalAccess(EntityUid uid, MechComponent component, ref GetAdditionalAccessEvent args)
    {
        var pilot = component.PilotSlot.ContainedEntity;
        if (pilot == null)
            return;

        args.Entities.Add(pilot.Value);
    }

    private void SetupUser(EntityUid mech, EntityUid pilot, MechComponent? component = null)
    {
        if (!Resolve(mech, ref component))
            return;

        var rider = EnsureComp<MechPilotComponent>(pilot);

        // Warning: this bypasses most normal interaction blocking components on the user, like drone laws and the like.
        var irelay = EnsureComp<InteractionRelayComponent>(pilot);

        _mover.SetRelay(pilot, mech);
        _interaction.SetRelay(pilot, mech, irelay);
        rider.Mech = mech;
        Dirty(pilot, rider);

        if (_net.IsClient)
            return;

        _actions.AddAction(pilot, ref component.MechCycleActionEntity, component.MechCycleAction, mech);
        _actions.AddAction(pilot, ref component.MechUiActionEntity, component.MechUiAction, mech);
        _actions.AddAction(pilot, ref component.MechEjectActionEntity, component.MechEjectAction, mech);
        _actions.AddAction(pilot, ref component.ToggleActionEntity, component.ToggleAction, mech); //Goobstation Mech Lights toggle action
    }

    private void RemoveUser(EntityUid mech, EntityUid pilot)
    {
        if (!RemComp<MechPilotComponent>(pilot))
            return;
        RemComp<RelayInputMoverComponent>(pilot);
        RemComp<InteractionRelayComponent>(pilot);

        _actions.RemoveProvidedActions(pilot, mech);
    }

    /// <summary>
    /// Destroys the mech, removing the user and ejecting all installed equipment.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    public virtual void BreakMech(EntityUid uid, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        TryEject(uid, component);
        var equipment = new List<EntityUid>(component.EquipmentContainer.ContainedEntities);
        foreach (var ent in equipment)
        {
            RemoveEquipment(uid, ent, component, forced: true);
        }

        component.Broken = true;
        UpdateAppearance(uid, component);
    }

    /// <summary>
    /// Cycles through the currently selected equipment.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    public void CycleEquipment(EntityUid uid, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var allEquipment = component.EquipmentContainer.ContainedEntities.ToList();

        var equipmentIndex = -1;
        if (component.CurrentSelectedEquipment != null)
        {
            bool StartIndex(EntityUid u) => u == component.CurrentSelectedEquipment;
            equipmentIndex = allEquipment.FindIndex(StartIndex);
        }

        equipmentIndex++;
        component.CurrentSelectedEquipment = equipmentIndex >= allEquipment.Count
            ? null
            : allEquipment[equipmentIndex];

        var popupString = component.CurrentSelectedEquipment != null
            ? Loc.GetString("mech-equipment-select-popup", ("item", component.CurrentSelectedEquipment))
            : Loc.GetString("mech-equipment-select-none-popup");

        if (_net.IsServer)
            _popup.PopupEntity(popupString, uid);

        Dirty(uid, component);
    }

    /// <summary>
    /// Inserts an equipment item into the mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toInsert"></param>
    /// <param name="component"></param>
    /// <param name="equipmentComponent"></param>
    public void InsertEquipment(EntityUid uid, EntityUid toInsert, MechComponent? component = null,
        MechEquipmentComponent? equipmentComponent = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Resolve(toInsert, ref equipmentComponent))
            return;

        if (component.EquipmentContainer.ContainedEntities.Count >= component.MaxEquipmentAmount)
            return;

        if (_whitelistSystem.IsWhitelistFail(component.EquipmentWhitelist, toInsert))
            return;

        equipmentComponent.EquipmentOwner = uid;
        _container.Insert(toInsert, component.EquipmentContainer);
        var ev = new MechEquipmentInsertedEvent(uid);
        RaiseLocalEvent(toInsert, ref ev);
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Removes an equipment item from a mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toRemove"></param>
    /// <param name="component"></param>
    /// <param name="equipmentComponent"></param>
    /// <param name="forced">Whether or not the removal can be cancelled</param>
    public void RemoveEquipment(EntityUid uid, EntityUid toRemove, MechComponent? component = null,
        MechEquipmentComponent? equipmentComponent = null, bool forced = false)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Resolve(toRemove, ref equipmentComponent))
            return;

        if (!forced)
        {
            var attemptev = new AttemptRemoveMechEquipmentEvent();
            RaiseLocalEvent(toRemove, ref attemptev);
            if (attemptev.Cancelled)
                return;
        }

        var ev = new MechEquipmentRemovedEvent(uid);
        RaiseLocalEvent(toRemove, ref ev);

        if (component.CurrentSelectedEquipment == toRemove)
            CycleEquipment(uid, component);

        equipmentComponent.EquipmentOwner = null;
        _container.Remove(toRemove, component.EquipmentContainer);
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Attempts to change the amount of energy in the mech.
    /// </summary>
    /// <param name="uid">The mech itself</param>
    /// <param name="delta">The change in energy</param>
    /// <param name="component"></param>
    /// <returns>If the energy was successfully changed.</returns>
    public virtual bool TryChangeEnergy(EntityUid uid, FixedPoint2 delta, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Energy + delta < 0)
            return false;

        component.Energy = FixedPoint2.Clamp(component.Energy + delta, 0, component.MaxEnergy);
        Dirty(uid, component);
        UpdateUserInterface(uid, component);
        return true;
    }

    /// <summary>
    /// Sets the integrity of the mech.
    /// </summary>
    /// <param name="uid">The mech itself</param>
    /// <param name="value">The value the integrity will be set at</param>
    /// <param name="component"></param>
    public void SetIntegrity(EntityUid uid, FixedPoint2 value, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Integrity = FixedPoint2.Clamp(value, 0, component.MaxIntegrity);

        if (component.Integrity <= 0)
        {
            BreakMech(uid, component);
        }
        else if (component.Broken)
        {
            component.Broken = false;
            UpdateAppearance(uid, component);
        }

        Dirty(uid, component);
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Checks if the pilot is present
    /// </summary>
    /// <param name="component"></param>
    /// <returns>Whether or not the pilot is present</returns>
    public bool IsEmpty(MechComponent component)
    {
        return component.PilotSlot.ContainedEntity == null;
    }

    /// <summary>
    /// Checks if an entity can be inserted into the mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toInsert"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool CanInsert(EntityUid uid, EntityUid toInsert, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        return IsEmpty(component) && _actionBlocker.CanMove(toInsert);
    }

    /// <summary>
    /// Updates the user interface
    /// </summary>
    /// <remarks>
    /// This is defined here so that UI updates can be accessed from shared.
    /// </remarks>
    public virtual void UpdateUserInterface(EntityUid uid, MechComponent? component = null)
    {
    }

    /// <summary>
    /// Attempts to insert a pilot into the mech.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="toInsert"></param>
    /// <param name="component"></param>
    /// <returns>Whether or not the entity was inserted</returns>
    public bool TryInsert(EntityUid uid, EntityUid? toInsert, MechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (toInsert == null || component.PilotSlot.ContainedEntity == toInsert)
            return false;

        if (!CanInsert(uid, toInsert.Value, component))
            return false;

        SetupUser(uid, toInsert.Value);
        _container.Insert(toInsert.Value, component.PilotSlot);
        UpdateAppearance(uid, component);
        UpdateHands(toInsert.Value, uid, true); // Goobstation
        return true;
    }

    /// <summary>
    /// Attempts to eject the current pilot from the mech
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="pilot">The pilot to eject</param>
    /// <returns>Whether or not the pilot was ejected.</returns>
    public bool TryEject(EntityUid uid, MechComponent? component = null, EntityUid? pilot = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.PilotSlot.ContainedEntity != null)
            pilot = component.PilotSlot.ContainedEntity.Value;

        if (pilot == null)
            return false;

        RemoveUser(uid, pilot.Value);
        _container.RemoveEntity(uid, pilot.Value);
        UpdateAppearance(uid, component);
        UpdateHands(pilot.Value, uid, false); // Goobstation
        return true;
    }

    // Goobstation Change Start
    private void UpdateHands(EntityUid uid, EntityUid mech, bool active)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (active)
            BlockHands(uid, mech, handsComponent);
        else
            FreeHands(uid, mech);
    }

    private void BlockHands(EntityUid uid, EntityUid mech, HandsComponent handsComponent)
    {
        var freeHands = 0;
        foreach (var hand in _hands.EnumerateHands(uid, handsComponent))
        {
            if (hand.HeldEntity == null)
            {
                freeHands++;
                continue;
            }

            // Is this entity removable? (they might have handcuffs on)
            if (HasComp<UnremoveableComponent>(hand.HeldEntity) && hand.HeldEntity != mech)
                continue;

            _hands.DoDrop(uid, hand, true, handsComponent);
            freeHands++;
            if (freeHands == 2)
                break;
        }
        if (_virtualItem.TrySpawnVirtualItemInHand(mech, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(mech, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid, EntityUid mech)
    {
        _virtualItem.DeleteInHandsMatching(uid, mech);
    }

    // Goobstation Change End
    private void OnGetMeleeWeapon(EntityUid uid, MechPilotComponent component, GetMeleeWeaponEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MechComponent>(component.Mech, out var mech))
            return;

        var weapon = mech.CurrentSelectedEquipment ?? component.Mech;
        args.Weapon = weapon;
        args.Handled = true;
    }

    private void OnCanAttackFromContainer(EntityUid uid, MechPilotComponent component, CanAttackFromContainerEvent args)
    {
        args.CanAttack = true;
    }

    private void OnAttackAttempt(EntityUid uid, MechPilotComponent component, AttackAttemptEvent args)
    {
        if (args.Target == component.Mech)
            args.Cancel();
    }

    // Goobstation: Prevent guns being used out of mechs if CCVAR is set.
    private void OnShotAttempted(EntityUid uid, MechEquipmentComponent component, ref ShotAttemptedEvent args)
    {
        if (!component.EquipmentOwner.HasValue
            || !HasComp<MechComponent>(component.EquipmentOwner.Value))
        {
            if (!_canUseMechGunOutside)
                args.Cancel();
            return;
        }

        var ev = new HandleMechEquipmentBatteryEvent();
        RaiseLocalEvent(uid, ev);
    }

    private void UpdateAppearance(EntityUid uid, MechComponent? component = null,
        AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref component, ref appearance, false))
            return;

        _appearance.SetData(uid, MechVisuals.Open, IsEmpty(component), appearance);
        _appearance.SetData(uid, MechVisuals.Broken, component.Broken, appearance);
    }

    private void OnDragDrop(EntityUid uid, MechComponent component, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Dragged, component.EntryDelay, new MechEntryEvent(), uid, target: uid)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnCanDragDrop(EntityUid uid, MechComponent component, ref CanDropTargetEvent args)
    {
        args.Handled = true;

        args.CanDrop |= !component.Broken && CanInsert(uid, args.Dragged, component);
    }

    private void OnEmagged(EntityUid uid, MechComponent component, ref GotEmaggedEvent args) // Goobstation
    {
        if (!component.BreakOnEmag)
            return;
        args.Handled = true;
        component.EquipmentWhitelist = null;
        Dirty(uid, component);
    }
}

/// <summary>
///     Event raised when the battery is successfully removed from the mech,
///     on both success and failure
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RemoveBatteryEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raised when a person removes someone from a mech,
///     on both success and failure
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MechExitEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raised when a person enters a mech, on both success and failure
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MechEntryEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raised when an user attempts to fire a mech weapon to check if its battery is drained
/// </summary>

[Serializable, NetSerializable]
public sealed partial class HandleMechEquipmentBatteryEvent : EntityEventArgs
{
}
