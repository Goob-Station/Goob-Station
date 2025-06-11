using System.Linq;
using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Storage;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.CloneProjector;

public sealed class SharedCloneProjectorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloneProjectorComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<CloneProjectorComponent, GetItemActionsEvent>(OnEquipped);
        SubscribeLocalEvent<CloneProjectorComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<CloneProjectorComponent, CloneProjectorActivatedEvent>(OnProjectorActivated);

        SubscribeLocalEvent<CloneComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnInit(Entity<CloneProjectorComponent> projector, ref MapInitEvent args)
    {
        projector.Comp.CloneContainer = _container.EnsureContainer<Container>(projector.Owner, "CloneContainer");
    }

    private void OnEquipped(Entity<CloneProjectorComponent> projector, ref GetItemActionsEvent args)
    {
        if (args.InHands
            || _net.IsClient)
            return;

        args.AddAction(ref projector.Comp.ActionEntity, projector.Comp.Action);

        var popup = Loc.GetString(projector.Comp.EquippedMessage);
        _popup.PopupEntity(popup, args.User, args.User);

        TryGenerateClone(projector, args.User);
    }

    private void OnUnequipped(Entity<CloneProjectorComponent> projector, ref GotUnequippedEvent args)
    {
        if (_net.IsClient)
            return;

        _actions.RemoveProvidedActions(args.Equipee, projector);
        TryInsertClone(projector);

        var popup = Loc.GetString(projector.Comp.UnequippedMessage);
        _popup.PopupEntity(popup, args.Equipee, args.Equipee);
    }
    private void OnProjectorActivated(Entity<CloneProjectorComponent> projector, ref CloneProjectorActivatedEvent args)
    {
        if (args.Handled
            || _net.IsClient)
            return;

        // Does the clone match the current user?
        var cloneMatches = projector.Comp.CurrentHost == args.Performer;
        var cloneGeneratedPopup = Loc.GetString(projector.Comp.CloneGeneratedMessage, ("user", Identity.Name(args.Performer, EntityManager)));

        if (cloneMatches)
        {
            // First, try to release a clone that already exists.
            if (TryDeployClone(projector))
            {
                args.Handled = true;
                _popup.PopupEntity(cloneGeneratedPopup, args.Performer, PopupType.Medium);
                return;
            }
        }

        // If there is no clone to release, try to insert the current clone.
        if (TryInsertClone(projector))
        {
            args.Handled = true;
            return;
        }

        // If there is no clone to release nor insert, create a new one.
        if (!TryGenerateClone(projector, args.Performer))
            return;

        TryDeployClone(projector);
        _popup.PopupEntity(cloneGeneratedPopup, args.Performer, PopupType.Medium);
        args.Handled = true;

    }

    private void OnMobStateChanged(Entity<CloneComponent> clone, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsIncapacitated(clone)
            || clone.Comp.HostProjector is not { } projector
            || _net.IsClient)
            return;

        TryInsertClone(projector);
        RaiseLocalEvent(clone, new RejuvenateEvent(true, false));

        if (projector.Comp.ActionEntity is { } actionEntity
            && TryComp<InstantActionComponent>(actionEntity, out var actionComp))
        {
            actionComp.Cooldown = (_timing.CurTime, _timing.CurTime + projector.Comp.DestroyedCooldown);

            _actions.UpdateAction(actionEntity, actionComp);
            Dirty(actionEntity, actionComp);
        }

        if (clone.Comp.HostEntity is not { } host)
            return;

        var destroyedPopup = Loc.GetString("gemini-projector-clone-destroyed");
        _popup.PopupEntity(destroyedPopup, host, host, PopupType.LargeCaution);
        _stun.TryParalyze(host, projector.Comp.StunDuration, true);

    }

    private bool TryGenerateClone(Entity<CloneProjectorComponent> projector, EntityUid performer)
    {
        if (!TryComp<HumanoidAppearanceComponent>(performer, out var appearance)
            || performer == projector.Comp.CurrentHost)
            return false;

        var speciesId = appearance.Species;

        if (!_protoManager.TryIndex(speciesId, out var species))
            return false;

        var clone = Spawn(species.Prototype, Transform(performer).Coordinates);

        if (projector.Comp.CloneUid is { } oldClone)
        {
            _container.TryRemoveFromContainer(oldClone);
            CleanItems(oldClone, true);

            if (_mind.TryGetMind(oldClone, out var id, out _))
                _mind.TransferTo(id, clone);

            QueueDel(oldClone);
        }

        _container.Insert(clone, projector.Comp.CloneContainer);

        _humanoidAppearance.CloneAppearance(performer, clone);

        if (projector.Comp.AddedComponents != null)
            EntityManager.AddComponents(clone, projector.Comp.AddedComponents);

        if (projector.Comp.RemovedComponents != null)
            EntityManager.RemoveComponents(clone, projector.Comp.RemovedComponents);

        projector.Comp.CurrentHost = performer;

        var cloneComp = EnsureComp<CloneComponent>(clone);

        cloneComp.HostProjector = projector;
        cloneComp.HostEntity = performer;

        _damageable.SetDamageModifierSetId(clone, projector.Comp.CloneDamageModifierSet);
        projector.Comp.CloneUid = clone;

        _meta.SetEntityName(clone, Identity.Name(performer, EntityManager) + " " + projector.Comp.NameSuffix);

        if (!TryEquipItems(projector))
            return false;

        Dirty(clone, projector.Comp);
        return true;
    }

    private bool TryInsertClone(CloneProjectorComponent projector)
    {
        if (projector.CloneUid is not { } clone
            || _container.IsEntityOrParentInContainer(clone))
            return false;

        _joints.RecursiveClearJoints(clone);
        CleanItems(clone);

        var cloneRetrievedPopup = Loc.GetString(projector.CloneRetrievedMessage, ("target", Name(clone)));
        _popup.PopupCoordinates(cloneRetrievedPopup, Transform(clone).Coordinates, PopupType.Medium);

        _container.Insert(clone, projector.CloneContainer);

        Dirty(clone, projector);
        return true;
    }

    private bool TryDeployClone(CloneProjectorComponent projector)
    {
        if (projector.CloneUid is not { } clone
            || !_container.IsEntityOrParentInContainer(clone))
            return false;

        Dirty(clone, projector);

        return _container.TryRemoveFromContainer(clone);
    }

    private bool TryEquipItems(CloneProjectorComponent projector)
    {
        if (projector.CloneUid is not { } clone
            || projector.CurrentHost is not { } host)
            return false;

        var toSpawn = new Dictionary<EntProtoId, string>();

        var hostInventory = _inventory.GetSlotEnumerator(host);
        while (hostInventory.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } item
                || _whitelist.IsWhitelistFail(projector.ClonedItemWhitelist, item)
                || _whitelist.IsBlacklistPass(projector.ClonedItemBlacklist, item))
                continue;

            var proto = Prototype(item);

            if (proto == null)
                continue;

            toSpawn.Add(proto, slot.ID);
        }

        if (toSpawn.Count <= 0)
            return true;

        foreach (var item in toSpawn
                     .Where(item => _inventory.SpawnItemInSlot(clone, item.Value, item.Key, true, true)))
        {
            if (_inventory.TryGetSlotEntity(clone, item.Value, out var spawnedItem))
                EnsureComp<UnremoveableComponent>(spawnedItem.Value);
        }

        return true;
    }

    private void CleanItems(EntityUid clone, bool removePocketItems = false)
    {
        var items = _inventory.GetSlotEnumerator(clone, SlotFlags.WITHOUT_POCKET);

        while (items.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } item)
                continue;

            if (TryComp<StorageComponent>(item, out var storageComponent))
            {
                foreach (var storedItem in _container.EmptyContainer(storageComponent.Container))
                    _physics.ApplyAngularImpulse(storedItem, ThrowingSystem.ThrowAngularImpulse);
            }

            if (_inventory.TryUnequip(clone, slot.ID, true))
                _physics.ApplyAngularImpulse(item, ThrowingSystem.ThrowAngularImpulse);
        }

        if (!removePocketItems)
            return;

        foreach (var pocketItem in _inventory.GetHandOrInventoryEntities(clone, SlotFlags.POCKET))
        {
            _container.TryRemoveFromContainer(pocketItem);
            _physics.ApplyAngularImpulse(pocketItem, ThrowingSystem.ThrowAngularImpulse);
        }

    }
}
