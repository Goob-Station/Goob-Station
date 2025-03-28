using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Pirates.Objectives;

public sealed partial class PirateObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObjectivePlunderComponent, ObjectiveGetProgressEvent>(GetPlunderProgress);
    }

    /// <summary>
    ///     Objective gets updated in <see cref=""/>
    /// </summary>
    private void GetPlunderProgress(Entity<ObjectivePlunderComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var tgt = _number.GetTarget(ent);
        if (tgt != 0)
            args.Progress = MathF.Min(ent.Comp.Plundered / tgt, 1f);
        else args.Progress = 1f;
    }
}
