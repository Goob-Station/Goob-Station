using Content.Goobstation.Common.SecondSkin;
using Content.Goobstation.Shared.SecondSkin;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SecondSkin;

public sealed partial class ModifyDisgustLevelSystem : EntityEffectSystem<DisgustComponent, ModifyDisgustLevel>
{
    protected override void Effect(Entity<DisgustComponent> entity, ref EntityEffectEvent<ModifyDisgustLevel> args)
    {
        var amount = args.Effect.Delta * args.Scale;

        if (amount == 0f)
            return;

        var ev = new ModifyDisgustEvent(amount);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ref ev);
    }
}

public sealed partial class ModifyDisgustLevel : EntityEffectBase<ModifyDisgustLevel>
{
    [DataField(required: true)]
    public float Delta;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (Delta == 0f)
            return null;

        var sign = MathF.Sign(Delta);
        return Loc.GetString("entity-effect-guidebook-modify-disgust",
            ("chance", Probability),
            ("deltasign", sign),
            ("amount", Delta * sign));
    }
}
