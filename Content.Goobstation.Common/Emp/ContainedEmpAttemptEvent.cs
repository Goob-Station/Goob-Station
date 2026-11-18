namespace Content.Goobstation.Common.Emp;

[ByRefEvent]
public record struct ContainedEmpAttemptEvent(EntityUid Target)
{
    public bool Cancelled;
}
