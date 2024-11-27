using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Heretic.Prototypes;
using System.Linq;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualBladeAscendBehavior : RitualSacrificeBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        var _body = args.EntityManager.System<SharedBodySystem>();

        var beheadedBodies = new List<EntityUid>();
        foreach (var uid in uids)
        {
            if (_body.GetBodyChildrenOfType(uid, BodyPartType.Head).Count() == 0)
                beheadedBodies.Add(uid);
        }

        if (beheadedBodies.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-blade");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        base.Finalize(args);
    }
}
