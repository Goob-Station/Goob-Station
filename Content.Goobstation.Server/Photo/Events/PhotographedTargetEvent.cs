namespace Content.Goobstation.Server.Photo;

[ByRefEvent]
public record struct PhotographedTargetEvent(EntityUid Photo, EntityUid User, List<EntityUid> OnPhoto);
