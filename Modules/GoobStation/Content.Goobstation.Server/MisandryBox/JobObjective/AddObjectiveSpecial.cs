using Content.Shared.Roles;

namespace Content.Goobstation.Server.MisandryBox.JobObjective;

public sealed partial class AddObjectiveSpecial : JobSpecial
{
    /// <summary>
    /// List of objective prototypes to assign to this job
    /// </summary>
    [DataField("objectives", required: true)]
    public List<string> Objectives = new();

    public override void AfterEquip(EntityUid mob)
    {
        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<JobObjectiveSystem>();
        system.QueueObjectives(mob, Objectives);
    }
}
