using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.Chat;

namespace Content.Server._Goobstation.Chat;

[RegisterComponent]
public sealed partial class ChatTriggerComponent : Component
{
    [DataField(required: true)]
    public List<ChatTriggerData> MessageData = new();
}

[DataDefinition]
public sealed partial class ChatTriggerData
{
    /// <summary>
    /// Message to check for
    /// </summary>
    [DataField(required: true)]
    public Dictionary<LocId, MessageCheckData> AllowedMessages;

    /// <summary>
    /// Whether chat sanitization manager should sanitize message if it matches.
    /// </summary>
    [DataField]
    public bool ShouldSanitize = true;

    /// <summary>
    /// Whether it should send radio message in case original message had radio prefix.
    /// </summary>
    [DataField]
    public bool ShouldSendRadio;

    /// <summary>
    /// In game IC Message types to check for.
    /// </summary>
    [DataField]
    public HashSet<InGameICChatType> DesiredICMessageTypes = new(); // This really should have Flags attribute, but I'm not going to modify it

    /// <summary>
    /// In game OOC Message types to check for.
    /// </summary>
    [DataField]
    public HashSet<InGameOOCChatType> DesiredOOCMessageTypes = new();

    /// <summary>
    /// Event tp raise when message matches.
    /// </summary>
    [DataField(required: true)]
    public BaseChatTriggerEvent Event;
}

[DataDefinition]
public sealed partial class MessageCheckData
{
    [DataField]
    public bool IgnoreCase = true;

    /// <summary>
    ///  Whether message should be equal to person spoken message or included in it
    /// </summary>
    [DataField]
    public bool CheckFullMessage;
}
