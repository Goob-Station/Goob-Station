// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Changeling.Objectives.Systems;

public sealed partial class ChangelingObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbConditionComponent, ObjectiveGetProgressEvent>(OnAbsorbGetProgress);
        SubscribeLocalEvent<StealDNAConditionComponent, ObjectiveGetProgressEvent>(OnStealDNAGetProgress);
        SubscribeLocalEvent<AbsorbChangelingConditionComponent, ObjectiveGetProgressEvent>(OnAbsorbChangelingGetProgress);
    }

    private void OnAbsorbGetProgress(EntityUid uid, AbsorbConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target != 0)
            args.Progress = MathF.Min(comp.Absorbed / target, 1f);
        else args.Progress = 1f;
    }
    private void OnStealDNAGetProgress(EntityUid uid, StealDNAConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target != 0)
            args.Progress = MathF.Min(comp.DNAStolen / target, 1f);
        else args.Progress = 1f;
    }
    private void OnAbsorbChangelingGetProgress(EntityUid uid, AbsorbChangelingConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (target != 0)
            args.Progress = MathF.Min(comp.LingAbsorbed / target, 1f);
        else
            args.Progress = 1f;
    }
}
