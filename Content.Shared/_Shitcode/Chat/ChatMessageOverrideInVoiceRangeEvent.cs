using Robust.Shared.Network;
using Content.Shared.Chat;

namespace Content.Shared._Shitcode.Chat;

    /// <summary>
    /// Raised for a specific entity to allow overriding or cancelling chat messages in voice range.
    /// all of this is Goob btw
    /// </summary>
    public sealed class ChatMessageOverrideInVoiceRangeEvent : EntityEventArgs
    {
        public ChatChannel Channel;
        public string Message;
        public string WrappedMessage;
        public bool Cancelled;

        public ChatMessageOverrideInVoiceRangeEvent(ChatChannel channel, string message, string wrappedMessage)
        {
            Channel = channel;
            Message = message;
            WrappedMessage = wrappedMessage;
            Cancelled = false;
        }
    }
