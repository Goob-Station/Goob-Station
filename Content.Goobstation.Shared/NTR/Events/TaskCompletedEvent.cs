using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskPrototype Task { get; }
    public EntityUid User { get; }
    public bool IsInstant { get; }

    public TaskCompletedEvent(NtrTaskPrototype task, EntityUid user, bool isInstant)
    {
        Task = task;
        User = user;
        IsInstant = isInstant;
    }
}
