namespace Content.Goobstation.Common.Chat;

/// <summary>
/// Used to handle generic chat messages with custom code
/// </summary>
/// <param name="Message">Formatted but now wrapped message</param>
/// <param name="Channel">ChatChannel</param>
[ByRefEvent]
public record struct BeforeChatMessageSentEvent(string Message, byte Channel)
{
    public bool Cancelled { get; private set; } = false;

    public void Cancel()
    {
        Cancelled = true;
    }
}
