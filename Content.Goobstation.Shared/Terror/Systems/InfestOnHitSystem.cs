using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Adds InfestedComponent to anyone hit by a WhiteTerror spider.
/// </summary>
public sealed class InfestOnHitSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<WhiteTerrorComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<WhiteTerrorComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!EntityManager.HasComponent<BodyComponent>(target))
                continue;

            EntityManager.EnsureComponent<InfestedComponent>(target);
        }
    }
}
