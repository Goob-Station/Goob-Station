using System.Reflection.Metadata.Ecma335;
using Content.Shared.Damage.Components;
using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Content.Shared.Standing;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class StaminaCritEntityConditionSystem : EntityConditionSystem<StaminaComponent, StaminaCritCondition>
{
    protected override void Condition(Entity<StaminaComponent> ent, ref EntityConditionEvent<StaminaCritCondition> args)
    {
        args.Result = ent.Comp.Critical;
    }
}

public sealed partial class StaminaCritCondition : EntityConditionBase<StaminaCritCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-stamcrit", ("inverted", Inverted));
    }
}
