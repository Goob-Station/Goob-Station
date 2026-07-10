// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Pirate.Loadouts; // Pirate: loadout
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared._Pirate.Photo; // Pirate: cameras (photo persistence)
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._EinsteinEngines.Silicon.IPC; // DeltaV
using Content.Shared.Whitelist; // Goobstation

namespace Content.Shared.Station;

public abstract class SharedStationSpawningSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly InventorySystem InventorySystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly InternalEncryptionKeySpawner _internalEncryption = default!; // DeltaV
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!; // Goobstation
    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<InventoryComponent> _inventoryQuery;
    private EntityQuery<StorageComponent> _storageQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _handsQuery = GetEntityQuery<HandsComponent>();
        _inventoryQuery = GetEntityQuery<InventoryComponent>();
        _storageQuery = GetEntityQuery<StorageComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    /// <summary>
    ///     Equips the data from a `RoleLoadout` onto an entity.
    /// </summary>
    public void EquipRoleLoadout(EntityUid entity, RoleLoadout loadout, RoleLoadoutPrototype roleProto)
    {
        // Order loadout selections by the order they appear on the prototype.
        #region Pirate: loadout
        var selectedLoadouts = new List<(Loadout Loadout, LoadoutPrototype Prototype)>();
        foreach (var group in loadout.SelectedLoadouts.OrderBy(x => roleProto.Groups.FindIndex(e => e == x.Key)))
        {
            // Deprecated groups validate saved profiles, but never spawn gear.
            if (group.Key.IsDeprecatedGroup(PrototypeManager))
                continue;

            foreach (var items in group.Value)
            {
                if (!PrototypeManager.TryIndex(items.Prototype, out var loadoutProto))
                {
                    Log.Error($"Unable to find loadout prototype for {items.Prototype}");
                    continue;
                }

                selectedLoadouts.Add((items, loadoutProto));
            }
        }

        // Backpack loadouts must equip before storage-only selections try to insert into them.
        foreach (var (items, loadoutProto) in selectedLoadouts.OrderByDescending(selected => HasEquipment(selected.Prototype)))
        {
            // Pirate: cameras (photo persistence)
            EquipStartingGear(entity, loadoutProto, raiseEvent: false, pirateFromSelectedLoadout: true, pirateLoadoutTint: items.CustomColorTint, pirateLoadoutName: items.CustomName, pirateLoadoutDescription: items.CustomDescription); // Pirate: loadout
        }
        #endregion

        EquipRoleName(entity, loadout, roleProto);
    }

    #region Pirate: loadout
    private bool HasEquipment(LoadoutPrototype loadout)
    {
        if (loadout.Equipment.Count > 0)
            return true;

        return loadout.StartingGear != null &&
               PrototypeManager.TryIndex(loadout.StartingGear, out StartingGearPrototype? startingGear) &&
               startingGear.Equipment.Count > 0;
    }
    private void ApplyLoadoutTint(EntityUid entity, string? tint)
    {
        if (string.IsNullOrEmpty(tint))
            return;

        var parsed = Color.TryFromHex(tint);
        if (!parsed.HasValue)
            return;

        var component = EnsureComp<LoadoutTintComponent>(entity);
        component.Color = parsed.Value;
        Dirty(entity, component);
    }

    private void ApplyLoadoutMetadata(EntityUid entity, string? name, string? description)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _metadata.SetEntityName(entity, name);

        if (!string.IsNullOrWhiteSpace(description))
            _metadata.SetEntityDescription(entity, description);
    }
    #endregion

    /// <summary>
    /// Applies the role's name as applicable to the entity.
    /// </summary>
    public void EquipRoleName(EntityUid entity, RoleLoadout loadout, RoleLoadoutPrototype roleProto)
    {
        string? name = null;

        if (roleProto.CanCustomizeName)
        {
            name = loadout.EntityName;
        }

        if (string.IsNullOrEmpty(name) && PrototypeManager.Resolve(roleProto.NameDataset, out var nameData))
        {
            name = Loc.GetString(_random.Pick(nameData.Values));
        }

        if (!string.IsNullOrEmpty(name))
        {
            _metadata.SetEntityName(entity, name);
        }
    }

    public void EquipStartingGear(
        EntityUid entity,
        LoadoutPrototype loadout,
        bool raiseEvent = true,
        bool pirateFromSelectedLoadout = false, // Pirate: cameras (photo persistence)
        string? pirateLoadoutTint = null, // Pirate: loadout
        string? pirateLoadoutName = null, // Pirate: loadout
        string? pirateLoadoutDescription = null) // Pirate: loadout
    {
        // Pirate: cameras (photo persistence)
        EquipStartingGear(entity, loadout.StartingGear, raiseEvent, pirateFromSelectedLoadout, pirateLoadoutTint, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout
        // Pirate: cameras (photo persistence)
        EquipStartingGear(entity, (IEquipmentLoadout) loadout, raiseEvent, pirateFromSelectedLoadout, pirateLoadoutTint, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout
    }

    /// <summary>
    /// <see cref="EquipStartingGear(Robust.Shared.GameObjects.EntityUid,System.Nullable{Robust.Shared.Prototypes.ProtoId{Content.Shared.Roles.StartingGearPrototype}},bool)"/>
    /// </summary>
    public void EquipStartingGear(
        EntityUid entity,
        ProtoId<StartingGearPrototype>? startingGear,
        bool raiseEvent = true,
        bool pirateFromSelectedLoadout = false, // Pirate: cameras (photo persistence)
        string? pirateLoadoutTint = null, // Pirate: loadout
        string? pirateLoadoutName = null, // Pirate: loadout
        string? pirateLoadoutDescription = null) // Pirate: loadout
    {
        PrototypeManager.Resolve(startingGear, out var gearProto);
        // Pirate: cameras (photo persistence)
        EquipStartingGear(entity, gearProto, raiseEvent, pirateFromSelectedLoadout, pirateLoadoutTint, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout
    }

    /// <summary>
    /// <see cref="EquipStartingGear(Robust.Shared.GameObjects.EntityUid,System.Nullable{Robust.Shared.Prototypes.ProtoId{Content.Shared.Roles.StartingGearPrototype}},bool)"/>
    /// </summary>
    public void EquipStartingGear(
        EntityUid entity,
        StartingGearPrototype? startingGear,
        bool raiseEvent = true,
        bool pirateFromSelectedLoadout = false, // Pirate: cameras (photo persistence)
        string? pirateLoadoutTint = null, // Pirate: loadout
        string? pirateLoadoutName = null, // Pirate: loadout
        string? pirateLoadoutDescription = null) // Pirate: loadout
    {
        // Begin DeltaV Additions: Fix nukie IPCs not having comms
        if (startingGear is not { } proto)
            return;

        _internalEncryption.TryInsertEncryptionKey(entity, proto);
        // End DeltaV Additions
        // Pirate: cameras (photo persistence)
        EquipStartingGear(entity, (IEquipmentLoadout?) startingGear, raiseEvent, pirateFromSelectedLoadout, pirateLoadoutTint, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout
    }

    /// <summary>
    /// Equips starting gear onto the given entity.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="startingGear">Starting gear to use.</param>
    /// <param name="raiseEvent">Should we raise the event for equipped. Set to false if you will call this manually</param>
    public void EquipStartingGear(
        EntityUid entity,
        IEquipmentLoadout? startingGear,
        bool raiseEvent = true,
        bool pirateFromSelectedLoadout = false, // Pirate: cameras (photo persistence)
        string? pirateLoadoutTint = null, // Pirate: loadout
        string? pirateLoadoutName = null, // Pirate: loadout
        string? pirateLoadoutDescription = null) // Pirate: loadout
    {
        if (startingGear == null)
            return;

        var xform = _xformQuery.GetComponent(entity);

        if (InventorySystem.TryGetSlots(entity, out var slotDefinitions))
        {
            foreach (var slot in slotDefinitions)
            {
                var equipmentStr = startingGear.GetGear(slot.Name);
                if (!string.IsNullOrEmpty(equipmentStr))
                {
                    var equipmentEntity = Spawn(equipmentStr, xform.Coordinates);
                    // Pirate: cameras (photo persistence)
                    RaiseSelectedLoadoutEntitySpawned(equipmentEntity, entity, pirateFromSelectedLoadout);
                    ApplyLoadoutTint(equipmentEntity, pirateLoadoutTint); // Pirate: loadout
                    ApplyLoadoutMetadata(equipmentEntity, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout
                    if (slot.Whitelist != null && !_whitelist.IsWhitelistPass(slot.Whitelist, equipmentEntity)) // Goob Change - Plasmamen
                    {
                        QueueDel(equipmentEntity);
                        continue;
                    }
                    InventorySystem.TryEquip(entity, equipmentEntity, slot.Name, silent: true, force: true);
                }
            }
        }

        if (_handsQuery.TryComp(entity, out var handsComponent))
        {
            var inhand = startingGear.Inhand;
            var coords = xform.Coordinates;
            foreach (var prototype in inhand)
            {
                var inhandEntity = Spawn(prototype, coords);
                // Pirate: cameras (photo persistence)
                RaiseSelectedLoadoutEntitySpawned(inhandEntity, entity, pirateFromSelectedLoadout);
                ApplyLoadoutTint(inhandEntity, pirateLoadoutTint); // Pirate: loadout
                ApplyLoadoutMetadata(inhandEntity, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout

                if (_handsSystem.TryGetEmptyHand((entity, handsComponent), out var emptyHand))
                {
                    _handsSystem.TryPickup(entity, inhandEntity, emptyHand, checkActionBlocker: false, handsComp: handsComponent);
                }
            }
        }

        if (startingGear.Storage.Count > 0)
        {
            var coords = _xformSystem.GetMapCoordinates(entity);
            _inventoryQuery.TryComp(entity, out var inventoryComp);

            foreach (var (slotName, entProtos) in startingGear.Storage)
            {
                if (entProtos == null || entProtos.Count == 0)
                    continue;

                if (inventoryComp != null &&
                    InventorySystem.TryGetSlotEntity(entity, slotName, out var slotEnt, inventoryComponent: inventoryComp) &&
                    _storageQuery.TryComp(slotEnt, out var storage))
                {

                    foreach (var entProto in entProtos)
                    {
                        var spawnedEntity = Spawn(entProto, coords);
                        // Pirate: cameras (photo persistence)
                        RaiseSelectedLoadoutEntitySpawned(spawnedEntity, entity, pirateFromSelectedLoadout);
                        ApplyLoadoutTint(spawnedEntity, pirateLoadoutTint); // Pirate: loadout
                        ApplyLoadoutMetadata(spawnedEntity, pirateLoadoutName, pirateLoadoutDescription); // Pirate: loadout

                        _storage.Insert(slotEnt.Value, spawnedEntity, out _, storageComp: storage, playSound: false);
                    }
                }
            }
        }

        if (raiseEvent)
        {
            var ev = new StartingGearEquippedEvent(entity);
            RaiseLocalEvent(entity, ref ev);
        }
    }

    #region Pirate: cameras (photo persistence)
    private void RaiseSelectedLoadoutEntitySpawned(EntityUid spawnedEntity, EntityUid owner, bool pirateFromSelectedLoadout)
    {
        if (!pirateFromSelectedLoadout)
            return;

        var ev = new SelectedLoadoutEntitySpawnedEvent(owner);
        RaiseLocalEvent(spawnedEntity, ev);
    }
    #endregion

    /// <summary>
    ///     Gets all the gear for a given slot when passed a loadout.
    /// </summary>
    /// <param name="loadout">The loadout to look through.</param>
    /// <param name="slot">The slot that you want the clothing for.</param>
    /// <returns>
    ///     If there is a value for the given slot, it will return the proto id for that slot.
    ///     If nothing was found, will return null
    /// </returns>
    public string? GetGearForSlot(RoleLoadout? loadout, string slot)
    {
        if (loadout == null)
            return null;

        foreach (var group in loadout.SelectedLoadouts)
        {
            if (group.Key.IsDeprecatedGroup(PrototypeManager)) // Pirate: loadout
                continue; // Pirate: loadout

            foreach (var items in group.Value)
            {
                if (!PrototypeManager.Resolve(items.Prototype, out var loadoutPrototype))
                    return null;

                var gear = ((IEquipmentLoadout) loadoutPrototype).GetGear(slot);
                if (gear != string.Empty)
                    return gear;
            }
        }

        return null;
    }
}
