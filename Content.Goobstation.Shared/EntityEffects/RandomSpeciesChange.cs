using System.Linq;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class RandomSpeciesChangeSystem
    : EntityEffectSystem<HumanoidAppearanceComponent, RandomSpeciesChange>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;

    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<RandomSpeciesChange> args)
    {
        var effect = args.Effect;

        var species = _proto.EnumeratePrototypes<SpeciesPrototype>();

        if (effect.SpeciesWhitelist != null && effect.SpeciesWhitelist.Count > 0)
            species = species.Where(q => effect.SpeciesWhitelist.Contains(q.ID));

        if (effect.SpeciesBlacklist != null && effect.SpeciesBlacklist.Count > 0)
            species = species.Where(q => !effect.SpeciesBlacklist.Contains(q.ID));

        var list = species.ToList();
        if (list.Count == 0)
            return;

        var sce = new SpeciesChange(_random.Pick(list).ID);
        _effects.TryApplyEffect(entity.Owner, sce);
    }
}

public sealed partial class RandomSpeciesChange : EntityEffectBase<RandomSpeciesChange>
{
    [DataField] public List<ProtoId<SpeciesPrototype>>? SpeciesWhitelist;
    [DataField] public List<ProtoId<SpeciesPrototype>>? SpeciesBlacklist;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species-random");
}
