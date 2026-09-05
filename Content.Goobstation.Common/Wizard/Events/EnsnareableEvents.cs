namespace Content.Goobstation.Common.Wizard.Events;

[ByRefEvent]
public record struct EnsnareableFreeAttemptEvent(EntityUid User, EntityUid Target)
{
    public readonly EntityUid User = User;
    public readonly EntityUid Target = Target;
    public bool Cancelled = false;
}

[ByRefEvent]
public record struct EnsnareableModifyDurationEvent(EntityUid User, EntityUid Target, float Duration);
