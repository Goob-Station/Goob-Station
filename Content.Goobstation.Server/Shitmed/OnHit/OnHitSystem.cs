// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shitmed.OnHit;
using Content.Shared.Cuffs.Components;

namespace Content.Goobstation.Server.Shitmed.OnHit;

public sealed partial class OnHitSystem : SharedOnHitSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CuffsOnHitComponent, CuffsOnHitDoAfter>(OnCuffsOnHitDoAfter);
        base.Initialize();
    }
    private void OnCuffsOnHitDoAfter(Entity<CuffsOnHitComponent> ent, ref CuffsOnHitDoAfter args)
    {
        if (!args.Args.Target.HasValue || args.Handled || args.Cancelled) return;

        var user = args.Args.User;
        var target = args.Args.Target.Value;

        if (!TryComp<CuffableComponent>(target, out var cuffable) || cuffable.Container.Count != 0)
            return;

        args.Handled = true;

        var handcuffs = SpawnNextToOrDrop(ent.Comp.HandcuffPrototype, args.User);

        if (!_cuffs.TryAddNewCuffs(target, user, handcuffs, cuffable))
            QueueDel(handcuffs);
    }
}