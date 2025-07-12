// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction;
using Content.Shared.Storage;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Storage;

public sealed class RelayInteractionStorageSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RelayInteractionStorageComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<RelayInteractionStorageComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target == null
            || _whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Target.Value)
            || !TryComp<StorageComponent>(ent, out var storage))
            return;

        foreach (var item in storage.Container.ContainedEntities)
            _interaction.InteractDoAfter(args.User, item, args.Target, Transform(args.Target.Value).Coordinates, true);
    }
}
