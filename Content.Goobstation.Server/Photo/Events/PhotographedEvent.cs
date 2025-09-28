namespace Content.Goobstation.Server.Photo;

[ByRefEvent]
public record struct PhotographedEvent(EntityUid Photo, List<EntityUid> OnPhoto);
