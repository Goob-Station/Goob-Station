// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

public sealed partial class HealNearOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealNearOnPrayComponent, NullrodPrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, HealNearOnPrayComponent comp, ref NullrodPrayEvent args)
    {
        var lookup = _lookup.GetEntitiesInRange(args.User, comp.Range);

        foreach (var entity in lookup.Where(entity => !HasComp<WeakToHolyComponent>(entity))) // im linqing it
        {
            if (HasComp<MobStateComponent>(entity)) //god forgive me I don't know linq
                _damageable.TryChangeDamage(entity, comp.Damage);
        }
    }
}
