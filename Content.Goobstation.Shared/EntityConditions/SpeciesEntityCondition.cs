using Content.Shared.EntityConditions;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class SpeciesEntityConditionSystem : EntityConditionSystem<HumanoidAppearanceComponent, SpeciesCondition>
{
    protected override void Condition(Entity<HumanoidAppearanceComponent> entity, ref EntityConditionEvent<SpeciesCondition> args)
    {
        args.Result = args.Condition.Species.Contains(entity.Comp.Species);
    }
}

public sealed partial class SpeciesCondition : EntityConditionBase<SpeciesCondition>
{
    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> Species = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
