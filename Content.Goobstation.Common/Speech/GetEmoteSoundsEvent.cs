namespace Content.Goobstation.Common.Speech;

[ByRefEvent]
public record struct GetEmoteSoundsEvent(string? EmoteSoundProtoId = null);
