using Content.Goobstation.Shared.InternalResources.Data;

namespace Content.Goobstation.Shared.InternalResources.Events;

[ByRefEvent]
public record struct GetInternalResourcesCostModifierEvent(EntityUid Target, InternalResourcesData Data, float Multiplier = 1);
