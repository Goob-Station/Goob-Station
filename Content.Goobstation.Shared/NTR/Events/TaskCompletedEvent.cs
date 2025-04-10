using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskPrototype Task;
    public EntityUid Performer;
    public bool IsInstant { get; set; }

    public TaskCompletedEvent(NtrTaskPrototype task, EntityUid performer)
    {
        Task = task;
        Performer = performer;
    }
}
