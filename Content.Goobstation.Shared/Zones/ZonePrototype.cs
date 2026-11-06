using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Zones;


[Prototype]
public sealed partial class ZonePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
