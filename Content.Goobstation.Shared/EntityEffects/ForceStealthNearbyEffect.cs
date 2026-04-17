using Content.Goobstation.Shared.Stealth;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class ForceStealthNearbyEffectSystem : EntityEffectSystem<ReactiveComponent, ForceStealthNearbyEffect>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ForcedStealthSystem _stealth = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<ForceStealthNearbyEffect> args)
    {
        foreach (var target in _lookup.GetEntitiesInRange(entity.Owner, args.Effect.Radius))
        {
            if (args.Effect.Chance >= 1f || _random.Prob(args.Effect.Chance))
            {
                _stealth.TryApplyForceStealth(target, out _, args.Effect.Duration);
            }
        }
    }
}

public sealed partial class ForceStealthNearbyEffect : EntityEffectBase<ForceStealthNearbyEffect>
{
    [DataField] public float Radius = 5f;

    [DataField] public float Duration = 5f;

    [DataField] public float Chance = 1f;

    public override bool ShouldLog => true;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-stealth-entities");
}
