using Content.Shared.EntityEffects;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class RandomSpeciesChange : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var protMan = IoCManager.Resolve<IPrototypeManager>();
        var random = IoCManager.Resolve<IRobustRandom>();
        var entityEffects = args.EntityManager.System<EntityEffectSystem>();

        // whatever, go my rngesus
        var species = protMan.EnumeratePrototypes<SpeciesPrototype>();
        var sce = new SpeciesChange
        {
            NewSpecies = random.Pick(species.ToList()).ID,
        };

        entityEffects.Effect(sce, args);
    }
}
