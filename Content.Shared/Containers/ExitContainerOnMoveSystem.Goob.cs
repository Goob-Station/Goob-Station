using Content.Shared.ActionBlocker;

namespace Content.Shared.Containers;

public sealed partial class ExitContainerOnMoveSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    private bool CanExit(EntityUid uid)
    {
        return _actionBlocker.CanMove(uid);
    }
}
