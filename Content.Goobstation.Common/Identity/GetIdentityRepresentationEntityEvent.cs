namespace Content.Goobstation.Common.Identity;

[ByRefEvent]
public record struct GetIdentityRepresentationEntityEvent(EntityUid? Uid = null);
