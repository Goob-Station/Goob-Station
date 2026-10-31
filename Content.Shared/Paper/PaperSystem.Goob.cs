using Content.Goobstation.Common.Paper;

namespace Content.Shared.Paper;

public sealed partial class PaperSystem
{
    private void GoobRaiseStamped(EntityUid paper, EntityUid user)
    {
        var ev = new PaperStampedEvent(paper);
        RaiseLocalEvent(user, ref ev);
    }
}
