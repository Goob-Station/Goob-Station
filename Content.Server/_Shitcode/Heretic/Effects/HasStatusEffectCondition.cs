using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class HasStatusEffect : EntityEffectCondition
{
    [DataField(required: true)]
    public EntProtoId EffectProto;

    [DataField]
    public bool Invert;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        return Invert ^ args.EntityManager.System<StatusEffectsSystem>()
            .HasStatusEffect(args.TargetEntity, EffectProto);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-guidebook-has-status-effect",
            ("effect", prototype.Index(EffectProto).Name),
            ("invert", Invert));
    }
}
