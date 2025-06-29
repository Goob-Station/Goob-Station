// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Weapons.MissChance;

public sealed class MissChanceSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MissChanceComponent, PreventCollideEvent>(PreventCollide);
    }

    private void PreventCollide(Entity<MissChanceComponent> ent, ref PreventCollideEvent args)
    {
        // This piece of goidacode guarantees synchronized random
        var random = new Random((int) _timing.CurTick.Value + (int) GetNetEntity(ent));

        if (args.Cancelled
        || !HasComp<MobStateComponent>(args.OtherEntity)
        || !random.Prob(ent.Comp.Chance))
            return;

        args.Cancelled = true;
    }

    public void ApplyMissChance(EntityUid? ent, float chance)
    {
        // GunShotEvent goes nuts with ammo uids on client so we tell it to stfu
        if (_netManager.IsClient || ent == null)
            return;

        var missComp = EnsureComp<MissChanceComponent>((EntityUid)ent);
        missComp.Chance = chance;
    }
}
