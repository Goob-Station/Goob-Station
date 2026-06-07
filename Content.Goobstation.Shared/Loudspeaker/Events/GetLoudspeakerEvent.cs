namespace Content.Goobstation.Shared.Loudspeaker.Events;

[ByRefEvent]
public record struct GetLoudspeakerEvent(
    List<EntityUid> Loudspeakers);
