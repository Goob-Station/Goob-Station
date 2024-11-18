using Content.Server.Objectives.Components;
using Content.Server.Revolutionary.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class NotCommandRequirementSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotCommandRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, NotCommandRequirementComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        // cheap equivalent to checking that job department is command, since all command members require admin notification when leaving
        if (_job.MindTryGetJob(args.MindId, out var prototype) && prototype.RequireAdminNotify)
            args.Cancelled = true;
    }
}
