using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskPrototype Task;
    public EntityUid Performer;

    public TaskCompletedEvent(NtrTaskPrototype task, EntityUid performer)
    {
        Task = task;
        Performer = performer;
    }
}
