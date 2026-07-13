// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles checking the job blacklist for this objective.
/// </summary>
public sealed class NotJobRequirementSystem : EntitySystem
{
    [Dependency] private readonly SharedJobSystem _jobs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotJobRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, NotJobRequirementComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        _jobs.MindTryGetJob(args.MindId, out var proto);


        // Goob start MisandryBox - JobObjectives - inverted behaviour
        if (proto is null)
            return;

        var hasJob = comp.Jobs.Contains(proto.ID);

        if (comp.Inverted ? !hasJob : hasJob)
            args.Cancelled = true;
        // Goob end
    }
}
