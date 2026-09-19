namespace Content.Goobstation.Common.Chat;

/// <summary>
/// Raised at an entity after its IC chat message is sanitized but before it is sent, letting
/// systems reroute or suppress it entirely (e.g. hallucinations only their victim can hear).
/// </summary>
[ByRefEvent]
public record struct BeforeChatMessageSentEvent(string Message, byte Channel)
{
    public bool Cancelled;
}
