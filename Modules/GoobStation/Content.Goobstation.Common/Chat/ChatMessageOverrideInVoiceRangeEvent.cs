namespace Content.Goobstation.Common.Chat;

[ByRefEvent]
public record struct ChatMessageOverrideInVoiceRange(bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}
