// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Rounding;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Server.Storage.EntitySystems;

public sealed class StorageFillVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StorageFillVisualizerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StorageFillVisualizerComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<StorageFillVisualizerComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnStartup(EntityUid uid, StorageFillVisualizerComponent component, ComponentStartup args)
    {
        UpdateAppearance(uid, component: component);
    }

    private void OnInserted(EntityUid uid, StorageFillVisualizerComponent component, EntInsertedIntoContainerMessage args)
    {
        UpdateAppearance(uid, component: component);
    }

    private void OnRemoved(EntityUid uid, StorageFillVisualizerComponent component, EntRemovedFromContainerMessage args)
    {
        UpdateAppearance(uid, component: component);
    }

    private void UpdateAppearance(EntityUid uid, StorageComponent? storage = null, AppearanceComponent? appearance = null,
        StorageFillVisualizerComponent? component = null)
    {
        if (!Resolve(uid, ref storage, ref appearance, ref component, false))
            return;

        if (component.MaxFillLevels < 1)
            return;

        if (!_appearance.TryGetData<int>(uid, StorageVisuals.StorageUsed, out var used, appearance))
            return;

        if (!_appearance.TryGetData<int>(uid, StorageVisuals.Capacity, out var capacity, appearance))
            return;

        var level = ContentHelpers.RoundToLevels(used, capacity, component.MaxFillLevels);
        _appearance.SetData(uid, StorageFillVisuals.FillLevel, level, appearance);
    }
}