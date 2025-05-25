// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Storage.Components;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared.Storage.EntitySystems;

/// <summary>
/// <c>ItemMapperSystem</c> is a system that on each initialization, insertion, removal of an entity from
/// given <see cref="ItemMapperComponent"/> (with appropriate storage attached) will check each stored item to see
/// if its tags/component, and overall quantity match <see cref="ItemMapperComponent.MapLayers"/>.
/// </summary>
[UsedImplicitly]
public abstract class SharedItemMapperSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemMapperComponent, ComponentInit>(InitLayers);
        SubscribeLocalEvent<ItemMapperComponent, EntInsertedIntoContainerMessage>(MapperEntityInserted);
        SubscribeLocalEvent<ItemMapperComponent, EntRemovedFromContainerMessage>(MapperEntityRemoved);
    }

    private void InitLayers(EntityUid uid, ItemMapperComponent component, ComponentInit args)
    {
        foreach (var (layerName, val) in component.MapLayers)
        {
            val.Layer = layerName;
        }

        if (EntityManager.TryGetComponent(uid, out AppearanceComponent? appearanceComponent))
        {
            var list = new List<string>(component.MapLayers.Keys);
            _appearance.SetData(uid, StorageMapVisuals.InitLayers, new ShowLayerData(list), appearanceComponent);
        }

        // Ensure appearance is correct with current contained entities.
        UpdateAppearance(uid, component);
    }

    private void MapperEntityRemoved(EntityUid uid, ItemMapperComponent itemMapper, EntRemovedFromContainerMessage args)
    {
        if (itemMapper.ContainerWhitelist != null && !itemMapper.ContainerWhitelist.Contains(args.Container.ID))
            return;

        UpdateAppearance(uid, itemMapper);
    }

    private void MapperEntityInserted(EntityUid uid,
        ItemMapperComponent itemMapper,
        EntInsertedIntoContainerMessage args)
    {
        if (itemMapper.ContainerWhitelist != null && !itemMapper.ContainerWhitelist.Contains(args.Container.ID))
            return;

        UpdateAppearance(uid, itemMapper);
    }

    private void UpdateAppearance(EntityUid uid, ItemMapperComponent? itemMapper = null)
    {
        if (!Resolve(uid, ref itemMapper))
            return;

        if (EntityManager.TryGetComponent(uid, out AppearanceComponent? appearanceComponent)
            && TryGetLayers(uid, itemMapper, out var containedLayers))
        {
            _appearance.SetData(uid,
                StorageMapVisuals.LayerChanged,
                new ShowLayerData(containedLayers),
                appearanceComponent);
        }
    }

    /// <summary>
    /// Method that iterates over storage of the entity in <paramref name="uid"/> and sets <paramref name="showLayers"/>
    /// according to <paramref name="itemMapper"/> definition. It will have O(n*m) time behavior
    /// (n - number of entities in container, and m - number of definitions in <paramref name="showLayers"/>).
    /// </summary>
    /// <param name="uid">EntityUid used to search the storage</param>
    /// <param name="itemMapper">component that contains definition used to map
    /// <see cref="EntityWhitelist">Whitelist</see> in <see cref="ItemMapperComponent.MapLayers"/> to string.
    /// </param>
    /// <param name="showLayers">list of <paramref name="itemMapper"/> layers that should be visible</param>
    /// <returns>false if <c>msg.Container.Owner</c> is not a storage, true otherwise.</returns>
    private bool TryGetLayers(EntityUid uid, ItemMapperComponent itemMapper, out List<string> showLayers)
    {
        var containedLayers = _container.GetAllContainers(uid)
            .Where(c => itemMapper.ContainerWhitelist?.Contains(c.ID) ?? true)
            .SelectMany(cont => cont.ContainedEntities)
            .ToArray();

        var list = new List<string>();
        foreach (var mapLayerData in itemMapper.MapLayers.Values)
        {
            var count = containedLayers.Count(ent => _whitelistSystem.IsWhitelistPassOrNull(mapLayerData.Whitelist,
                ent));
            if (count >= mapLayerData.MinCount && count <= mapLayerData.MaxCount)
            {
                list.Add(mapLayerData.Layer);
            }
        }

        showLayers = list;
        return true;
    }
}