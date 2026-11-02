using Content.Shared.Medical;
using Content.Goobstation.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Damage.System;

public sealed partial class VomitOnHitSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private VomitSystem _vomit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VomitOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, VomitOnHitComponent comp, MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target))
                continue;

            if (!_random.Prob(comp.Chance))
                continue;

            _vomit.Vomit(target, comp.ThirstAdded, comp.HungerAdded, comp.Force);
        }
    }
}
