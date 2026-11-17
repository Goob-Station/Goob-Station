using Content.Goobstation.Shared.MartialArts.Specific;
using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class CapoeiraVelocityEntityConditionSystem : EntityConditionSystem<PhysicsComponent, CapoeiraVelocityCondition>
{
    [Dependency] private CapoeiraSystem _capoeira = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    protected override void Condition(Entity<PhysicsComponent> ent, ref EntityConditionEvent<CapoeiraVelocityCondition> args)
    {
        args.Result = _capoeira.CanPerformCapoeiraMove(
            ent.AsNullable(),
            args.Condition.MinPower,
            args.Condition.MaxPower,
            args.Condition.MinVelocity,
            args.Condition.VelocityPowerMultiplier,
            out _
        );

        if (!args.Result)
            _popup.PopupEntity(Loc.GetString("capoeira-fail-low-velocity"), ent, ent); // todo slop shouldnt be here
    }
}

public sealed partial class CapoeiraVelocityCondition : EntityConditionBase<CapoeiraVelocityCondition>
{
    [DataField(required: true)]
    public float MinVelocity;

    [DataField]
    public float MinPower = 1f;

    [DataField]
    public float MaxPower = 4f;

    [DataField]
    public float VelocityPowerMultiplier = 0.6f;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return "Placeholder";
    }
}
