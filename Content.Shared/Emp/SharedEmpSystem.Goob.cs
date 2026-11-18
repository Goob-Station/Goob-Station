using Content.Goobstation.Common.Emp;
using Robust.Shared.Containers;

namespace Content.Shared.Emp;

public abstract partial class SharedEmpSystem
{
    [Dependency] private readonly SharedContainerSystem _goobContainer = default!;

    private bool IsEmpWardedByContainer(EntityUid uid)
    {
        var current = uid;
        while (_goobContainer.TryGetContainingContainer((current, null, null), out var container))
        {
            current = container.Owner;
            var ev = new ContainedEmpAttemptEvent(uid);
            RaiseLocalEvent(current, ref ev);
            if (ev.Cancelled)
                return true;
        }

        return false;
    }
}
