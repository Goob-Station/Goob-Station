using Content.Goobstation.Common.Wizard.Events;

namespace Content.Client.Ghost;

public sealed partial class GhostSystem
{
    public bool AreGhostsForcedVisible()
    {
        var ev = new GetCanSeeGhostsEvent();
        RaiseLocalEvent(ref ev);
        return ev.Can;
    }
}