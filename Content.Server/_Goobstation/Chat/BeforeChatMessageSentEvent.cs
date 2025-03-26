using Content.Server.Chat.Systems;

namespace Content.Server._Goobstation.Chat;

public sealed class BeforeChatMessageSentEvent(string message, bool hasRadioPrefix, InGameICChatType? icType, InGameOOCChatType? oocType)
    : CancellableEntityEventArgs
{
    public bool ShouldSanitize = true;

    public bool HasRadioPrefix = hasRadioPrefix;

    public string Message = message;

    public InGameICChatType? ICChatType = icType;

    public InGameOOCChatType? OOCChatType = oocType;
}
