using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind;

[ByRefEvent]
public record struct HivemindGetNameEvent(ProtoId<CollectiveMindPrototype> Hivemind, string? Name = null);
