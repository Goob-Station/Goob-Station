using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReactionEffects;

[DataDefinition]
public sealed partial class ChangeGlimmerReactionEffect : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-glimmer-reaction-effect", ("chance", Probability),
            ("count", Count));

    /// <summary>
    ///     Added to glimmer when reaction occurs.
    /// </summary>
    [DataField]
    public float Count = 1;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs _)
            return;

        var glimmerSystem = args.EntityManager.EntitySysManager.GetEntitySystem<GlimmerSystem>();

        glimmerSystem.DeltaGlimmerInput(Count);
    }
}
