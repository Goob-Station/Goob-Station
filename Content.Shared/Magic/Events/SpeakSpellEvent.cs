using Content.Shared.Chat;

namespace Content.Shared.Magic.Events;

[ByRefEvent]
public readonly struct SpeakSpellEvent(EntityUid performer, string speech, InGameICChatType chatType)
{
    public readonly EntityUid Performer = performer;
    public readonly string Speech = speech;
    public readonly InGameICChatType ChatType = chatType;
}
