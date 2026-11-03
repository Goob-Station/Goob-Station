using Content.Shared.EntityConditions;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class DepartmentEntityConditionSystem : EntityConditionSystem<MindContainerComponent, DepartmentCondition>
{
    [Dependency] private SharedJobSystem _jobs = default!;

    protected override void Condition(Entity<MindContainerComponent> entity, ref EntityConditionEvent<DepartmentCondition> args)
    {
        if (!_jobs.MindTryGetJobId(entity.Comp.Mind, out var job)
            || job is not { } jobId
            || !_jobs.TryGetAllDepartments(jobId.Id, out var departments))
            return;

        foreach (var department in departments)
        {
            if (!args.Condition.Departments.Contains(department.ID))
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class DepartmentCondition : EntityConditionBase<DepartmentCondition>
{
    [DataField(required: true)]
    public List<ProtoId<DepartmentPrototype>> Departments = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
