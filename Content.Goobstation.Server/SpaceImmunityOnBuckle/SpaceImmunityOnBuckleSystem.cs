// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.SpaceImmunityOnBuckle;
using Content.Goobstation.Shared.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Buckle.Components;


namespace Content.Goobstation.Server.SpaceImmunityOnBuckle;

public sealed class SpaceImmunityOnBuckleSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpaceImmunityOnBuckleComponent, StrappedEvent>(OnBuckled);
        SubscribeLocalEvent<SpaceImmunityOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnBuckled(Entity<SpaceImmunityOnBuckleComponent> ent, ref StrappedEvent args)
    {
        EnsureComp<PressureImmunityComponent>(args.Buckle.Owner);
        EnsureComp<SpecialLowTempImmunityComponent>(args.Buckle.Owner);
    }

    private void OnUnstrapped(Entity<SpaceImmunityOnBuckleComponent> ent, ref UnstrappedEvent args)
    {
        RemComp<PressureImmunityComponent>(args.Buckle.Owner);
        RemComp<SpecialLowTempImmunityComponent>(args.Buckle.Owner);
    }

}
