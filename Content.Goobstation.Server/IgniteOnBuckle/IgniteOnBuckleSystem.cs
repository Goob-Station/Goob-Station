// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.IgniteOnBuckle;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Buckle.Components;


namespace Content.Goobstation.Server.IgniteOnBuckle;

public sealed class IgniteOnBuckleSystem : EntitySystem
{

    [Dependency] private readonly FlammableSystem _flammable = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IgniteOnBuckleComponent, StrappedEvent>(OnBuckled);
    }

    private void OnBuckled(Entity<IgniteOnBuckleComponent> ent, ref StrappedEvent args)
    {
        if (!TryComp<FlammableComponent>(args.Buckle, out var flammable))
            return;

        flammable.FireStacks += ent.Comp.FireStacks;
        _flammable.Ignite(args.Buckle, args.Strap, flammable);
    }

}
