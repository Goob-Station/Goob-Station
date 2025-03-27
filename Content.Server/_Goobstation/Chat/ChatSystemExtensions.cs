using Content.Server.Chat.Systems;

namespace Content.Server._Goobstation.Chat;

public static class ChatSystemExtensions
{
    public static bool BeforeInGameICMessageSent(this ChatSystem chatSystem,
        EntityUid source,
        InGameICChatType desiredType,
        ref string message,
        ref bool checkRadioPrefix,
        out bool shouldSanitize,
        IEntityManager entityManager)
    {
        shouldSanitize = true;
        var beforeMessageSentEv = new BeforeChatMessageSentEvent(message.Trim(), checkRadioPrefix, desiredType, null);
        entityManager.EventBus.RaiseLocalEvent(source, beforeMessageSentEv);
        if (beforeMessageSentEv.Cancelled)
            return false;
        message = beforeMessageSentEv.Message;
        checkRadioPrefix = beforeMessageSentEv.HasRadioPrefix;
        shouldSanitize = beforeMessageSentEv.ShouldSanitize;
        return true;
    }

    public static bool BeforeInGameOOCMessageSent(this ChatSystem chatSystem,
        EntityUid source,
        InGameOOCChatType sendType,
        ref string message,
        IEntityManager entityManager)
    {
        var beforeMessageSentEv = new BeforeChatMessageSentEvent(message, false, null, sendType);
        entityManager.EventBus.RaiseLocalEvent(source, beforeMessageSentEv);
        if (beforeMessageSentEv.Cancelled)
            return false;
        message = beforeMessageSentEv.Message;
        return true;
    }
}
