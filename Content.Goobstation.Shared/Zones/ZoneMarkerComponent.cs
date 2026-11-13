using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Zones;

[RegisterComponent]
public sealed partial class ZoneMarkerComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ZonePrototype> ZoneId;

    [DataField(required: true)]
    public float Range;
}
