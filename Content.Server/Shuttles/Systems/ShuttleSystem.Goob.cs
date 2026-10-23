using Content.Goobstation.Common.Shuttles;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private void RaiseShuttleCrushed(EntityUid crushed, EntityUid shuttle)
    {
        var ev = new ShuttleCrushedEvent(shuttle);
        RaiseLocalEvent(crushed, ref ev);
    }
}
