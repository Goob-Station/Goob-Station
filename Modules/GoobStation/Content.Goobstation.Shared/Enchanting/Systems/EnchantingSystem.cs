// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Provides API for enchanting with <see cref="EnchantComponent"/> and <see cref="EnchantedComponent"/>.
/// </summary>
public sealed class EnchantingSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<EnchantComponent> _query;
    private EntityQuery<EnchantedComponent> _enchantedQuery;
    private EntityQuery<ItemComponent> _itemQuery;
    private EntityQuery<StackComponent> _stackQuery;
    private Dictionary<EntProtoId<EnchantComponent>, EnchantComponent> _enchants = new();
    private HashSet<Entity<EnchantingTableComponent>> _tables = new();
    private HashSet<Entity<EnchanterComponent>> _enchanters = new();
    private HashSet<Entity<EnchantedComponent>> _enchantedItems = new();

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<EnchantComponent>();
        _enchantedQuery = GetEntityQuery<EnchantedComponent>();
        _itemQuery = GetEntityQuery<ItemComponent>();
        _stackQuery = GetEntityQuery<StackComponent>();

        SubscribeLocalEvent<EnchantedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<EnchantedComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        CacheEnchants();
    }

    private void OnInit(Entity<EnchantedComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }

    private void OnExamined(Entity<EnchantedComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(EnchantedComponent)))
        {
            foreach (var uid in ent.Comp.Enchants)
            {
                var comp = _query.Comp(uid);
                var key = comp.ShowLevel ? "enchant-examine-level" : "enchant-examine";
                args.PushMarkup(Loc.GetString(key, ("enchant", uid), ("level", comp.Level)));
            }
        }
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            CacheEnchants();
    }

    private void CacheEnchants()
    {
        _enchants.Clear();
        var factory = EntityManager.ComponentFactory;
        foreach (var proto in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.TryGetComponent<EnchantComponent>(out var comp, factory))
                _enchants.Add(proto.ID, comp);
        }
    }

    #region Public API

    /// <summary>
    /// Get the prototype's <see cref="EnchantComponent"/> for an enchant id.
    /// </summary>
    public EnchantComponent? GetEnchantData(EntProtoId<EnchantComponent> id)
    {
        if (_enchants.TryGetValue(id, out var data))
            return data;

        return null;
    }

    /// <summary>
    /// Returns true if an enchant can be applied to an item.
    /// </summary>
    public bool CanEnchant(Entity<EnchantedComponent?> item, EntProtoId<EnchantComponent> id)
    {
        // no giving yourself unbreaking, only hamlet
        if (!_itemQuery.HasComp(item))
            return false;

        // just no
        if (_stackQuery.HasComp(item))
            return false;

        // invalid enchant id passed
        if (GetEnchantData(id) is not {} data)
        {
            Log.Error($"Tried to enchant {ToPrettyString(item)} with invalid enchant {id}");
            return false;
        }

        // item needs to be whitelisted
        if (!_whitelist.CheckBoth(item, blacklist: data.Blacklist, whitelist: data.Whitelist))
            return false;

        // if the item isn't enchanted it's good to go
        if (!_enchantedQuery.Resolve(ref item, false))
            return true;

        // can't have incompatible enchants
        var comp = item.Comp!;
        foreach (var incompatible in data.Incompatible)
        {
            if (FindEnchant(comp, incompatible) != null)
                return false;
        }

        // enchant is at max level
        if (FindEnchant(comp, id) is {} enchant)
            return !enchant.Comp.IsMaxed;

        // item can't be enchanted further
        return comp.Tier > comp.Enchants.Count;
    }

    /// <summary>
    /// Find an enchant with the same name as a prototype.
    /// This is how duplicate enchants are prevented
    /// </summary>
    public Entity<EnchantComponent>? FindEnchant(EnchantedComponent comp, EntProtoId<EnchantComponent> id)
    {
        // bad prototype
        if (_proto.Index(id).Name is not {} name)
        {
            Log.Error($"Enchant prototype {id} has no name set!");
            return null;
        }

        foreach (var enchant in comp.Enchants)
        {
            if (Name(enchant) == name)
                return (enchant, _query.Comp(enchant));
        }

        // none found unlucky
        return null;
    }

    /// <summary>
    /// Set the enchanting tier of an item, adding Enchanting if it doesn't have it.
    /// </summary>
    public void SetTier(EntityUid item, int tier)
    {
        var comp = EnsureComp<EnchantedComponent>(item);
        if (comp.Tier == tier)
            return;

        comp.Tier = tier;
        Dirty(item, comp);
    }

    /// <summary>
    /// Increases an item's enchant tier if it isn't at max already.
    /// Does nothing if it isn't enchanted already.
    /// </summary>
    public bool TryUpgradeTier(Entity<EnchantedComponent> item)
    {
        if (item.Comp.Tier >= item.Comp.MaxTier)
            return false;

        item.Comp.Tier++;
        Dirty(item);
        return true;
    }

    /// <summary>
    /// Add or upgrade an enchant with a specific level.
    /// If an enchant would get over max level it gets clamped, wasting the excess.
    /// </summary>
    public bool Enchant(EntityUid item, EntProtoId<EnchantComponent> id, int level = 1)
    {
        if (!CanEnchant(item, id))
            return false;

        // first check if there is already an existing enchant to upgrade
        var added = !EnsureComp<EnchantedComponent>(item, out var comp);
        if (FindEnchant(comp, id) is {} enchant)
        {
            var oldLevel = enchant.Comp.Level;
            enchant.Comp.Level = Math.Min(enchant.Comp.Level + level, enchant.Comp.MaxLevel);
            Dirty(enchant);

            var upgrade = new EnchantUpgradedEvent(enchant.Comp, item, oldLevel);
            RaiseLocalEvent(enchant, ref upgrade);
            return true;
        }

        // spawn a new one
        if (!TrySpawnInContainer(id, item, comp.ContainerId, out var spawned))
        {
            Log.Error($"Failed to spawn enchant {id} for {ToPrettyString(item)}!");
            // don't make it shiny without any enchants
            if (added)
                RemComp<EnchantedComponent>(item);
            return false;
        }

        AddEnchant(spawned.Value, item, level);
        return true;
    }

    /// <summary>
    /// Find an enchanting table near an item.
    /// </summary>
    public EntityUid? FindTable(EntityUid item)
    {
        var coords = Transform(item).Coordinates;
        _tables.Clear();
        _lookup.GetEntitiesInRange<EnchantingTableComponent>(coords, range: 0.5f, _tables);
        return _tables.Count > 0 ? _tables.First() : null;
    }

    /// <summary>
    /// Find an enchanter near an item.
    /// This won't return itself.
    /// </summary>
    public Entity<EnchanterComponent>? FindEnchanter(EntityUid item)
    {
        var coords = Transform(item).Coordinates;
        _enchanters.Clear();
        _lookup.GetEntitiesInRange<EnchanterComponent>(coords, range: 0.5f, _enchanters);
        foreach (var ent in _enchanters)
        {
            if (ent.Owner != item)
                return ent;
        }

        return null;
    }

    /// <summary>
    /// Find all enchanted items near a table.
    /// The returned hashset is reused between calls and must not be modified.
    /// </summary>
    public HashSet<Entity<EnchantedComponent>> FindEnchantedItems(EntityUid table)
    {
        var coords = Transform(table).Coordinates;
        _enchantedItems.Clear();
        _lookup.GetEntitiesInRange<EnchantedComponent>(coords, range: 0.5f, _enchantedItems);
        return _enchantedItems;
    }

    /// <summary>
    /// Get the entity assigned to an enchant, or null if it has none/is invalid.
    /// </summary>
    public EntityUid? GetEnchantedItem(EntityUid enchant)
    {
        return _query.CompOrNull(enchant)?.Enchanted;
    }

    #endregion

    private void AddEnchant(EntityUid uid, EntityUid item, int level)
    {
        var comp = _query.Comp(uid);
        comp.Enchanted = item;
        comp.Level = Math.Min(level, comp.MaxLevel);
        Dirty(uid, comp);

        var ev = new EnchantAddedEvent(comp, item);
        RaiseLocalEvent(uid, ref ev);
    }
}
