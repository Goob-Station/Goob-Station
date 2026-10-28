using Content.Shared.Chat;
using Content.Shared.Speech;

namespace Content.Server.Chat.Systems;

public sealed partial class ChatSystem
{
    private SpeechVerbPrototype GetSpeechVerbWithOverride(EntityUid source, string message)
    {
        var nameEv = new TransformSpeakerNameEvent(source, Name(source));
        RaiseLocalEvent(source, nameEv);

        if (nameEv.SpeechVerb != null && _prototypeManager.Resolve(nameEv.SpeechVerb, out var proto))
            return proto;

        return GetSpeechVerb(source, message);
    }
}
