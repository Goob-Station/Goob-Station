using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Adds InfestedComponent to anyone hit by a WhiteTerror spider.
/// </summary>
public sealed class InfestOnHitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<TerrorSpiderComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<TerrorSpiderComponent> ent, ref MeleeHitEvent args)
    {
        var proto = _proto.Index(ent.Comp.SpiderType);

        if (!proto.InfestsOnHit)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<BodyComponent>(target))
                continue;

            EnsureComp<InfestedComponent>(target);
        }
    }
}
