using Content.Shared.EntityConditions;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class StatusEffectEntityConditionSystem : EntityConditionSystem<StatusEffectContainerComponent, StatusEffectCondition>
{
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    protected override void Condition(Entity<StatusEffectContainerComponent> entity, ref EntityConditionEvent<StatusEffectCondition> args)
    {
        args.Result = _statusEffects.HasStatusEffect(entity, args.Condition.Effect);
    }
}

public sealed partial class StatusEffectCondition : EntityConditionBase<StatusEffectCondition>
{
    [DataField(required: true)]
    public EntProtoId Effect;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
