namespace Content.Goobstation.Common.Chat;

[ByRefEvent]
public record struct BeforeChatMessageSentEvent(string Message, byte Channel)
{
    public bool Cancelled { get; private set; } = false;

    public void Cancel()
    {
        Cancelled = true;
    }
}
