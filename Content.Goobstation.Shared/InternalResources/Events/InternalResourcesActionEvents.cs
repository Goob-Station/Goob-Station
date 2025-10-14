namespace Content.Goobstation.Shared.InternalResources.Events;

[ByRefEvent]
public record struct GetInternalResourcesCostModifierEvent(EntityUid Target, float Multiplier = 1);
