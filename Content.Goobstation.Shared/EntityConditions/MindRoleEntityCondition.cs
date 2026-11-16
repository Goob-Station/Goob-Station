using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class MindRoleEntityConditionSystem : EntityConditionSystem<MindContainerComponent, MindRoleCondition>
{
    [Dependency] private SharedRoleSystem _roles = default!;

    protected override void Condition(Entity<MindContainerComponent> entity, ref EntityConditionEvent<MindRoleCondition> args)
    {
        if (entity.Comp.Mind is not { } mindId || !TryComp<MindComponent>(mindId, out var mind))
            return;

        args.Result = _roles.MindHasRole((mindId, mind), args.Condition.Whitelist);
    }
}

public sealed partial class MindRoleCondition : EntityConditionBase<MindRoleCondition>
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
