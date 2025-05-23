// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Wieldable.Components;

namespace Content.Goobstation.Shared.Wieldable;

public sealed class GoobSharedWieldableSystem : EntitySystem
{
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WieldableComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentShutdown(Entity<WieldableComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (ent.Comp.ApplyNewPrefixOnShutdown)
            ent.Comp.OldInhandPrefix = ent.Comp.NewPrefixOnShutdown;

        _virtualItem.DeleteInHandsMatching(Transform(ent).ParentUid, ent);
    }
}
