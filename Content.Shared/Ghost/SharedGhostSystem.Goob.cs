using Content.Goobstation.Common.Wizard.Events;

namespace Content.Shared.Ghost;

public abstract partial class SharedGhostSystem
{
    public bool AreGhostsForcedVisible()
    {
        var ev = new GetCanSeeGhostsEvent();
        RaiseLocalEvent(ref ev);
        return ev.Can;
    }
}