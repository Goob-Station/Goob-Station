namespace Content.Goobstation.Common.Surgery;

[ByRefEvent]
public readonly record struct OrganTransplantedEvent(EntityUid Patient, EntityUid Organ);
