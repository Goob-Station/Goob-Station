using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._CorvaxGoob.BookOfGreentext;

public sealed partial class WashCurseOfGreentextReaction : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-wash-curse-of-greentext-reaction", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<CurseOfBookOfGreentextComponent>(args.TargetEntity, out var curse)) return;

        args.EntityManager.RemoveComponent(args.TargetEntity, curse);
    }
}
