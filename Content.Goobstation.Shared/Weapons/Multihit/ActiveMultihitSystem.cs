// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Weapons.Multihit;

public sealed class ActiveMultihitSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveMultihitComponent, MeleeHitEvent>(OnHit, after: new[] { typeof(MultihitSystem) });
    }

    private void OnHit(Entity<ActiveMultihitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        if (Math.Abs(ent.Comp.DamageMultiplier - 1f) > 0.01f)
        {
            var modifierSet = new DamageModifierSet
            {
                Coefficients = args.BaseDamage.DamageDict
                    .Select(x => new KeyValuePair<string, float>(x.Key, ent.Comp.DamageMultiplier))
                    .ToDictionary(),
            };

            args.ModifiersList.Add(modifierSet);
        }

        RemComp(ent.Owner, ent.Comp);
    }

}
