// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SetSelector;

public sealed class SetSelectorSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SetSelectorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SetSelectorComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<SetSelectorComponent, SetSelectorApproveMessage>(OnApprove);
        SubscribeLocalEvent<SetSelectorComponent, SetSelectorChangeSetMessage>(OnChangeSet);
    }

    private static void OnMapInit(Entity<SetSelectorComponent> selector, ref MapInitEvent args)
    {
        if (selector.Comp.SetsToSelect == -1)
        {
            selector.Comp.AvailableSets = selector.Comp.PossibleSets;
            return;
        }

        var sets = selector.Comp.PossibleSets.ToArray();
        new System.Random().Shuffle(sets);
        selector.Comp.AvailableSets = sets.Take(selector.Comp.SetsToSelect).ToList();
    }

    private void OnUIOpened(Entity<SetSelectorComponent> selector, ref BoundUIOpenedEvent args)
    {
        UpdateUI(selector.Owner, selector.Comp);
    }

    private void OnApprove(Entity<SetSelectorComponent> selector, ref SetSelectorApproveMessage args)
    {
        if (selector.Comp.SelectedSets.Count != selector.Comp.MaxSelectedSets)
            return;

        EntityUid spawnedStorage = default;
        var storagePrototype = selector.Comp.SpawnedStoragePrototype;
        var spawnedStorageContainer = selector.Comp.SpawnedStorageContainer;
        var openSpawnedStorage = selector.Comp.OpenSpawnedStorage;
        var coordinates = _transform.GetMapCoordinates(selector.Owner);
        _container.TryGetContainingContainer(selector, out var target);
        List<string> ignoredContainers = new() { "implant", "pocket1", "pocket2", "pocket3", "pocket4" };

        List<EntityUid> spawnedEntities = [];

        foreach (var setIndex in selector.Comp.SelectedSets)
        {
            var set = _proto.Index(selector.Comp.AvailableSets[setIndex]);

            // Spawn guaranteed content
            spawnedEntities.AddRange(set.Content.Select(item => Spawn(item, coordinates)));

            // Spawn from entity tables
            foreach (var tableId in set.Tables)
            {
                var tablePrototype = _proto.Index(tableId);
                var tableSpawns = _entityTable.GetSpawns(tablePrototype.Table);
                spawnedEntities.AddRange(tableSpawns.Select(spawn => Spawn(spawn, coordinates)));
            }
        }

        _audio.PlayPvs(selector.Comp.ApproveSound, Transform(selector.Owner).Coordinates);
        Del(selector);

        if (storagePrototype != null && spawnedStorageContainer != null)
        {
            spawnedStorage = Spawn(storagePrototype, coordinates);
            RecursiveInsert(spawnedStorage, target, ignoredContainers);
            _container.TryGetContainer(spawnedStorage, spawnedStorageContainer, out target);
        }

        ignoredContainers.AddRange(_hands.EnumerateHands(args.Actor).Select(hand => hand.Name));
        spawnedEntities.ForEach(ent => RecursiveInsert(ent, target, ignoredContainers));

        if (openSpawnedStorage)
            _entityStorage.OpenStorage(spawnedStorage);
    }

    private bool RecursiveInsert(EntityUid ent, BaseContainer? container, List<string> ignoredContainers)
    {
        if (container == null)
            return false;

        if (!ignoredContainers.Contains(container.ID) && _container.Insert((ent, null, null, null), container))
            return true;

        if (Transform(container.Owner).ParentUid.IsValid()
        && _container.TryGetContainingContainer(container.Owner, out var newContainer))
            return RecursiveInsert(ent, newContainer, ignoredContainers);

        return false;
    }

    private void OnChangeSet(Entity<SetSelectorComponent> selector, ref SetSelectorChangeSetMessage args)
    {
        if (!selector.Comp.SelectedSets.Remove(args.SetNumber))
             selector.Comp.SelectedSets.Add(args.SetNumber);

        UpdateUI(selector.Owner, selector.Comp);
    }

    private void UpdateUI(EntityUid uid, SetSelectorComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        Dictionary<int, SelectableSetInfo> data = new();

        for (var i = 0; i < component.AvailableSets.Count; i++)
        {
            var set = _proto.Index(component.AvailableSets[i]);
            var selected = component.SelectedSets.Contains(i);
            var info = new SelectableSetInfo(
                set.Name,
                set.Description,
                set.Sprite,
                selected);
            data.Add(i, info);
        }

        _ui.SetUiState(uid, SetSelectorUIKey.Key, new SetSelectorBoundUserInterfaceState(data, component.MaxSelectedSets));
    }
}
