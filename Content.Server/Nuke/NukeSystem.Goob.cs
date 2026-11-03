using Content.Goobstation.Common.Nuke;

namespace Content.Server.Nuke;

public sealed partial class NukeSystem
{
    private void GoobRaiseDisarmed(EntityUid nuke, EntityUid user)
    {
        var ev = new NukeDisarmedByEvent(nuke);
        RaiseLocalEvent(user, ref ev);
    }
}
