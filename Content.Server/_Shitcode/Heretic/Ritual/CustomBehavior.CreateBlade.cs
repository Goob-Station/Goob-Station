using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualCreateBladeBehavior : RitualCustomBehavior
{
    [DataField]
    public EntProtoId? BladeProto = "HereticBladeBase";

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        var entMan = args.EntityManager;
        if (!entMan.TryGetComponent(args.Performer, out HereticComponent? heretic))
            return false;

        RefreshBlades(heretic, entMan);
        if (heretic.OurBlades.Count < heretic.MaxBlades)
            return true;

        outstr = Loc.GetString("heretic-ritual-fail-limit");
        return false;
    }

    public override void Finalize(RitualData args)
    {
        var entMan = args.EntityManager;
        if (!entMan.TryGetComponent(args.Performer, out HereticComponent? heretic))
            return;

        RefreshBlades(heretic, entMan);
        if (heretic.OurBlades.Count >= heretic.MaxBlades)
            return;

        var blade = entMan.Spawn(BladeProto, entMan.System<TransformSystem>().GetMapCoordinates(args.Platform));
        heretic.OurBlades.Add(blade);
    }

    private void RefreshBlades(HereticComponent comp, IEntityManager entMan)
    {
        comp.OurBlades.RemoveAll(x => !entMan.EntityExists(x));
    }
}
