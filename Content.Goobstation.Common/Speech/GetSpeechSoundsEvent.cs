namespace Content.Goobstation.Common.Speech;

[ByRefEvent]
public record struct GetSpeechSoundEvent(string? SpeechSoundProtoId = null);
