namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskFailedEvent : EntityEventArgs
{
    public EntityUid User;
    public int Penalty;

    public TaskFailedEvent(EntityUid user, int penalty = 4)
    {
        User = user;
        Penalty = penalty;
    }
}
