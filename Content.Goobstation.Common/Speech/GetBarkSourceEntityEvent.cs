namespace Content.Goobstation.Common.Speech;

[ByRefEvent]
public record struct GetBarkSourceEntityEvent(EntityUid? Ent = null);
