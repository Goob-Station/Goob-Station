using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard;

[UsedImplicitly]
public sealed partial class HasComponentCondition : EntityEffectCondition
{
    [DataField(required: true)]
    public string Component = default!;

    [DataField(required: true)]
    public string GuidebookComponentName = default!;

    [DataField]
    public bool Invert;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        var hasComp = args.EntityManager.HasComponent(args.TargetEntity,
            args.EntityManager.ComponentFactory.GetRegistration(Component).Type);

        return hasComp ^ Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Invert));
    }
}
