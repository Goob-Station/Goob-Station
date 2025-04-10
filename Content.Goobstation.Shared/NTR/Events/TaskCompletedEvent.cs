namespace Content.Goobstation.Shared.NTR.Events;
public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskPrototype Task;
    public TaskCompletedEvent(NtrTaskPrototype task)
    {
        Task = task;
    }
}
