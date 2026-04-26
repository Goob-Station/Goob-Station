using Content.Shared.Speech;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Loudspeaker.Events;

[ByRefEvent]
public record struct GetLoudspeakerDataEvent(
    bool IsActive = false,
    int? FontSize = null,
    bool AffectRadio = false,
    bool AffectChat = false,
    string? Message = null,
    ProtoId<SpeechSoundsPrototype>? SpeechSounds = null);
