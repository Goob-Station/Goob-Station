// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

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
                    .ToDictionary(x => (ProtoId<DamageTypePrototype>) x.Key, _ => ent.Comp.DamageMultiplier),
            };

            args.ModifiersList.Add(modifierSet);
        }

        RemComp(ent.Owner, ent.Comp);
    }

}
