using Content.Goobstation.Shared.Damage.Components;
using Content.Shared.Medical;
using Content.Shared.Random.Helpers;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Damage.System;

public sealed partial class VomitOnHitSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private VomitSystem _vomit = default!;

    [SubscribeLocalEvent]
    private void OnMeleeHit(Entity<VomitOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!SharedRandomExtensions.PredictedProb(_timing, ent.Comp.Chance, GetNetEntity(target)))
                continue;

            if (ent.Comp.ThirstAdded is { } thirst && ent.Comp.HungerAdded is { } hunger)
            {
                _vomit.Vomit(target, thirst, hunger, ent.Comp.Force);
            }
            else
            {
                _vomit.Vomit(target, force: ent.Comp.Force);
            }
        }
    }
}
