namespace Content.Goobstation.Common.Chat;

[ByRefEvent]
public record struct ChatMessageOverrideInRange(bool RequiresHearing = false, bool RequiresSight = false, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}
