using Content.Server.Heretic.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualCanAscend : RitualCustomBehavior
{
    [DataField]
    public bool CheckObjectives = true;

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        var heretic = args.Mind.Comp;

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

        if (!CheckObjectives)
            return true;

        if (!args.EntityManager.System<HereticSystem>().ObjectivesAllowAscension(args.Mind))
        {
            outstr = Loc.GetString("heretic-ritual-fail-cannot-ascend-objectives");
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}
