using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualCanAscend : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        var entMan = args.EntityManager;
        if (!entMan.TryGetComponent(args.Performer, out HereticComponent? heretic))
            return false;

        if (heretic.Ascended)
        {
            outstr = Loc.GetString("heretic-ritual-fail-already-ascended");
            return false;
        }

        if (!heretic.CanAscend)
        {
            outstr = Loc.GetString("heretic-ritual-fail-cannot-ascend");
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}
