using System.Linq;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualLockAscendBehavior : RitualSacrificeBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        var targets = new List<EntityUid>();
        for (var i = 0; i < Max; i++)
        {
            var target = uids[i];
            if (IsTargetValid(target, args.EntityManager))
                targets.Add(target);
        }

        if (targets.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-lock");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        base.Finalize(args);
    }

    private bool IsTargetValid(EntityUid uid, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent(uid, out BodyComponent? body))
            return false;

        var bodySys = entMan.System<BodySystem>();

        if (!bodySys.TryGetRootPart(uid, out var rootPart, body))
            return false;

        return !bodySys.GetPartOrgans(rootPart.Value, rootPart.Value).Any();
    }
}
