// SPDX-FileCopyrightText: 2023 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Containers;

using Content.Shared.Item;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Toggleable;

namespace Content.Shared.ContainerHeld;

public sealed class ContainerHeldSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerHeldComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<ContainerHeldComponent, EntRemovedFromContainerMessage>(OnContainerModified);
    }

    private void OnContainerModified(EntityUid uid, ContainerHeldComponent comp, ContainerModifiedMessage args)
    {
        if (!(HasComp<StorageComponent>(uid)
              && TryComp<AppearanceComponent>(uid, out var appearance)
              && TryComp<ItemComponent>(uid, out var item)))
        {
            return;
        }
        if (_storage.GetCumulativeItemAreas(uid) >= comp.Threshold)
        {
            _item.SetHeldPrefix(uid, "full", component: item);
            _appearance.SetData(uid, ToggleVisuals.Toggled, true, appearance);
        }
        else
        {
            _item.SetHeldPrefix(uid, "empty", component: item);
            _appearance.SetData(uid, ToggleVisuals.Toggled, false, appearance);
        }
    }
}