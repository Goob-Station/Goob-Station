using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class InfestOnHitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<TerrorSpiderComponent> ent, ref MeleeHitEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SpiderType, out var proto)) return;

        if (!proto.CanInfestOnHit) return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<HumanoidAppearanceComponent>(target)) continue;

            if (_status.HasStatusEffect(target, "Infested")) continue;

            // just so the effect doesn't linger forever if something goes wrong.
            _status.TryAddStatusEffect(target, "Infested", out _, TimeSpan.FromMinutes(30));
        }
    }
}
