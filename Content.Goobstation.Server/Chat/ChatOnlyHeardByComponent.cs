namespace Content.Goobstation.Server.Chat;

/// <summary>
/// Suppresses this entity's IC chat (speech, whispers and emotes) for everyone except
/// the entity itself and the listener, who still needs to be in normal hearing range.
/// </summary>
[RegisterComponent]
public sealed partial class ChatOnlyHeardByComponent : Component
{
    /// <summary>
    /// The only entity besides the speaker that receives the messages.
    /// If null the speaker just talks to themself.
    /// </summary>
    [DataField]
    public EntityUid? Listener;

    /// <summary>
    /// Optional color applied to the speaker's name in spoken and whispered messages.
    /// </summary>
    [DataField]
    public Color? NameColor;
}
