namespace Content.Shared._Lavaland.Chasm;

[ByRefEvent]
public record struct BeforeChasmFallingEvent(EntityUid Entity, bool Cancelled = false);
