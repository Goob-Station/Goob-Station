using Content.Goobstation.Common.SecondSkin;
using Content.Goobstation.Shared.SecondSkin;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SecondSkin;

public sealed partial class ModifyDisgustLevel : EntityEffect
{
    [DataField(required: true)]
    public float Delta;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (Delta == 0f)
            return null;

        var sign = MathF.Sign(Delta);
        return Loc.GetString("entity-effect-guidebook-modify-disgust",
            ("chance", Probability),
            ("deltasign", sign),
            ("amount", Delta * sign));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out DisgustComponent? disgust))
            return;

        var amount = Delta;

        if (args is EntityEffectReagentArgs reagentArgs)
            amount *= reagentArgs.Scale.Float();

        if (amount == 0f)
            return;

        var ev = new ModifyDisgustEvent(amount);
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ref ev);
    }
}
