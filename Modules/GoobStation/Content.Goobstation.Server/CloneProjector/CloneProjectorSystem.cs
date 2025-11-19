// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.CloneProjector;
using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Server.Emp;
using Content.Server.Ghost.Roles.Components;
using Content.Shared._DV.Carrying;
using Content.Shared._EinsteinEngines.Silicon.IPC;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Holopad;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Storage;
using Content.Shared.Strip.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.CloneProjector;

public sealed partial class CloneProjectorSystem : SharedCloneProjectorSystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
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
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly CarryingSystem _carrying = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly MobThresholdSystem _thresholds = default!;
    [Dependency] private readonly InternalEncryptionKeySpawner _encryptionKeySpawner = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloneProjectorComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<CloneProjectorComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CloneProjectorComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        SubscribeLocalEvent<CloneProjectorComponent, GetItemActionsEvent>(OnEquipped);
        SubscribeLocalEvent<CloneProjectorComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<CloneProjectorComponent, CloneProjectorActivatedEvent>(OnProjectorActivated);

        SubscribeLocalEvent<WearingCloneProjectorComponent, MobStateChangedEvent>(OnWearerStateChanged);

        InitializeClone();

        _sawmill = Logger.GetSawmill("clone-projector");
    }

    private void OnInit(Entity<CloneProjectorComponent> projector, ref MapInitEvent args) =>
        projector.Comp.CloneContainer = _container.EnsureContainer<Container>(projector.Owner, "CloneContainer");

    private void OnExamined(Entity<CloneProjectorComponent> projector, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var status = Loc.GetString("clone-projector-examined-status", ("cloneStatus", projector.Comp.CloneUid != null));
        args.PushMarkup(status);

        if (!TryComp<DamageableComponent>(projector.Comp.CloneUid, out var damageable)
            || !_thresholds.TryGetDeadThreshold(projector.Comp.CloneUid.Value, out var deathThreshold))
            return;

        var remainingHealth = deathThreshold - damageable.TotalDamage;
        var health = Loc.GetString("clone-projector-examined-health", ("cloneHealth", remainingHealth / deathThreshold * 100 ));
        args.PushMarkup(health);
    }

    private void OnGetVerbs(Entity<CloneProjectorComponent> projector, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract
            || !args.CanComplexInteract
            || projector.Comp.CurrentHost is not { } host
            || args.User != host
            || !CanUseProjector(projector, args.User))
            return;

        AlternativeVerb regenerateVerb = new()
        {
            Act = () =>
            {
                TryGenerateClone(projector, host, true);
                DoCooldown(projector);
            },
            Text = Loc.GetString("gemini-projector-regenerate-verb"),
            Message = Loc.GetString("gemini-projector-regenerate-verb-text"),
            Icon = new SpriteSpecifier.Rsi(new("Mobs/Silicon/station_ai.rsi"), "default"),
            Priority = 2
        };

        AlternativeVerb rebootVerb = new()
        {
            Act = () =>
            {
                TryGenerateClone(projector, host, true, true);
                DoCooldown(projector);
            },
            Text = Loc.GetString("gemini-projector-reboot-verb"),
            Message = Loc.GetString("gemini-projector-reboot-verb-text"),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/modsuit.rsi"), "activate"),
            Priority = 2
        };

        args.Verbs.Add(regenerateVerb);
        args.Verbs.Add(rebootVerb);
    }

    private void OnEquipped(Entity<CloneProjectorComponent> projector, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;

        args.AddAction(ref projector.Comp.ActionEntity, projector.Comp.Action);

        var popup = Loc.GetString(projector.Comp.EquippedMessage);
        _popup.PopupEntity(popup, args.User, args.User);

        TryGenerateClone(projector, args.User);

        if (projector.Comp.DoStun)
            _stun.TryParalyze(args.User, projector.Comp.StunDuration, true);

        EnsureComp<WearingCloneProjectorComponent>(args.User).ConnectedProjector = projector;
    }

    private void OnUnequipped(Entity<CloneProjectorComponent> projector, ref GotUnequippedEvent args)
    {
        _actions.RemoveProvidedActions(args.Equipee, projector);
        TryInsertClone(projector);

        var popup = Loc.GetString(projector.Comp.UnequippedMessage);
        _popup.PopupEntity(popup, args.Equipee, args.Equipee);

        if (projector.Comp.DoStun)
            _stun.TryParalyze(args.Equipee, projector.Comp.StunDuration, true);

        RemComp<WearingCloneProjectorComponent>(args.Equipee);
    }


    private void OnProjectorActivated(Entity<CloneProjectorComponent> projector, ref CloneProjectorActivatedEvent args)
    {
        if (args.Handled
            || !CanUseProjector(projector, args.Performer))
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

    private void OnWearerStateChanged(Entity<WearingCloneProjectorComponent> wearer, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead
            || wearer.Comp.ConnectedProjector is not { } projector
            || projector.Comp.CloneUid is not { } clone)
            return;

        CleanClone(clone, true);
        TryInsertClone(projector);

    }
    private bool TryGenerateClone(Entity<CloneProjectorComponent> projector, EntityUid performer, bool force = false, bool removeMind = false)
    {
        if (!TryComp<HumanoidAppearanceComponent>(performer, out var appearance))
        {
            _sawmill.Error($"Could not resolve {nameof(HumanoidAppearanceComponent)} for {ToPrettyString(performer)}");
            return false;
        }

        if (performer == projector.Comp.CurrentHost
            && !force)
            return false;

        var speciesId = appearance.Species;

        if (!_protoManager.TryIndex(speciesId, out var species))
        {
            _sawmill.Error($"Failed to index species ID of {speciesId}");
            return false;
        }

        var clone = Spawn(species.Prototype, Transform(performer).Coordinates);

        if (projector.Comp.CloneUid is { } oldClone)
        {
            _container.TryRemoveFromContainer(oldClone);
            CleanClone(oldClone, true);

            if (_mind.TryGetMind(oldClone, out var id, out _) && !removeMind)
                _mind.TransferTo(id, clone);

            Del(oldClone);
        }

        _container.Insert(clone, projector.Comp.CloneContainer);

        _humanoidAppearance.CloneAppearance(performer, clone);

        if (projector.Comp.AddedComponents != null)
            EntityManager.AddComponents(clone, projector.Comp.AddedComponents);

        if (projector.Comp.RemovedComponents != null)
            EntityManager.RemoveComponents(clone, projector.Comp.RemovedComponents);

        projector.Comp.CurrentHost = performer;

        var cloneComp = EnsureComp<HolographicCloneComponent>(clone);

        cloneComp.HostProjector = projector;
        cloneComp.HostEntity = performer;

        _damageable.SetDamageModifierSetId(clone, projector.Comp.CloneDamageModifierSet);

        _meta.SetEntityName(clone, Identity.Name(performer, EntityManager) + " " + Loc.GetString(projector.Comp.NameSuffix));
        projector.Comp.CloneUid = clone;

        if (!TryEquipItems(projector))
        {
            _sawmill.Error($"Failed to equip items for holographic clone of {ToPrettyString(clone)}");
            return false;
        }

        var roleComp = EnsureComp<GhostRoleComponent>(clone);
        roleComp.RoleName = Loc.GetString(projector.Comp.GhostRoleName);
        roleComp.RoleDescription = Loc.GetString(projector.Comp.GhostRoleDescription);
        roleComp.RoleRules = Loc.GetString(projector.Comp.GhostRoleRules);

        Dirty(projector);
        return true;
    }

    public bool TryInsertClone(Entity<CloneProjectorComponent> projector, bool doCooldown = false)
    {
        if (projector.Comp.CloneUid is not { } clone
            || _container.IsEntityOrParentInContainer(clone))
            return false;

        CleanClone(clone);

        var cloneRetrievedPopup = Loc.GetString(projector.Comp.CloneRetrievedMessage, ("target", Name(clone)));
        _popup.PopupCoordinates(cloneRetrievedPopup, Transform(clone).Coordinates, PopupType.Medium);

        if (TerminatingOrDeleted(projector)
            || !_container.Insert(clone, projector.Comp.CloneContainer))
        {
            _sawmill.Error($"Failed to insert clone entity: {ToPrettyString(clone)} into {ToPrettyString(projector)}");

            QueueDel(clone);
            return false;
        }

        if (doCooldown)
            DoCooldown(projector);

        Dirty(projector);
        return true;
    }

    private bool TryDeployClone(CloneProjectorComponent projector)
    {
        if (projector.CloneUid is not { } clone
            || !_container.IsEntityOrParentInContainer(clone))
            return false;

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

            toSpawn[proto] = slot.ID;
        }

        if (toSpawn.Count <= 0)
            return true;

        // Make all equipped clothing unremovable after spawning to prevent duplication.
        foreach (var item in toSpawn
                     .Where(item => _inventory.SpawnItemInSlot(clone, item.Value, item.Key, true, true)))
        {
            if (_inventory.TryGetSlotEntity(clone, item.Value, out var spawnedItemNullable)
                && spawnedItemNullable is { } spawnedItem )
            {
                EnsureComp<UnremoveableComponent>(spawnedItem);
                if (!TryComp<ItemSlotsComponent>(spawnedItem, out var itemSlots))
                    continue;

                // Lock all slots to prevent duplication.
                foreach (var slot in itemSlots.Slots.Values)
                {
                    if (slot.ContainerSlot != null)
                        _itemSlots.SetLock(spawnedItem, slot, true);
                }
            }

        }

        // Spawn keys
        if (TryComp<EncryptionKeyHolderComponent>(host, out var hostKey)
            && TryComp<EncryptionKeyHolderComponent>(clone, out var cloneKey))
        {
            foreach (var key in hostKey.KeyContainer.ContainedEntities)
            {
                if (!TryPrototype(key, out var keyProto))
                    continue;

                SpawnInContainerOrDrop(keyProto.ID, clone, cloneKey.KeyContainer.ID);
            }
        }

        return true;
    }

    private void CleanClone(EntityUid clone, bool removePocketItems = false)
    {
        if (TerminatingOrDeleted(clone))
            return;

        // Clear all joints.
        _joints.RecursiveClearJoints(clone);

        // Drop held items.
        foreach (var heldItem in _handsSystem.EnumerateHeld(clone))
            _handsSystem.TryDrop(clone, heldItem);

        // Drop held entities.
        if (TryComp<CarryingComponent>(clone, out var carrying))
            _carrying.DropCarried(clone, carrying.Carried);

        // Drop all items inside of items equipped or held.
        var equippedItems = _inventory.GetSlotEnumerator(clone, SlotFlags.WITHOUT_POCKET);
        while (equippedItems.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } item)
                continue;

            if (HasComp<UnremoveableComponent>(item)
                && TryComp<StorageComponent>(item, out var storageComponent))
            {
                foreach (var storedItem in _container.EmptyContainer(storageComponent.Container))
                    _physics.ApplyAngularImpulse(storedItem, ThrowingSystem.ThrowAngularImpulse);
            }

            if (_inventory.TryUnequip(clone, slot.ID, true))
                _physics.ApplyAngularImpulse(item, ThrowingSystem.ThrowAngularImpulse);
        }

        if (!removePocketItems)
            return;

        // Drop all items inside your pockets.
        foreach (var pocketItem in _inventory.GetHandOrInventoryEntities(clone, SlotFlags.POCKET))
        {
            _container.TryRemoveFromContainer(pocketItem);
            _physics.ApplyAngularImpulse(pocketItem, ThrowingSystem.ThrowAngularImpulse);
        }

    }

    private void DoCooldown(Entity<CloneProjectorComponent> projector)
    {
        if (projector.Comp.ActionEntity is not { } actionEntity
            || !TryComp<ActionComponent>(actionEntity, out var actionComp))
            return;

        _actions.SetCooldown(projector.Owner, _timing.CurTime + projector.Comp.DestroyedCooldown);

        _actions.UpdateAction((actionEntity, actionComp));
        Dirty(actionEntity, actionComp);
    }

    private bool CanUseProjector(Entity<CloneProjectorComponent> projector, EntityUid user)
    {
        return _whitelist.IsBlacklistFail(projector.Comp.UserBlacklist, user);
    }
}
