using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR;

public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskPrototype Task;

    public TaskCompletedEvent(NtrTaskPrototype task)
    {
        Task = task;
    }
}
