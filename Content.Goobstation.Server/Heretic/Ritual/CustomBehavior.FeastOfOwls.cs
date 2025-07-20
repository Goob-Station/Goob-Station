using Content.Goobstation.Server.Heretic.Ui;
using Content.Server.EUI;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Heretic.Ritual;

public sealed partial class RitualFeastOfOwlsBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        return true;
    }

    public override void Finalize(RitualData args)
    {
        if (!args.EntityManager.TryGetComponent(args.Performer, out HereticComponent? heretic)
            || heretic.Ascended
            || !heretic.CanAscend
            || !args.EntityManager.TryGetComponent(args.Performer, out ActorComponent? actor))
            return;

        var eui = IoCManager.Resolve<EuiManager>();
        eui.OpenEui(new FeastOfOwlsEui(args.Performer, args.Platform, args.EntityManager), actor.PlayerSession);
    }
}
