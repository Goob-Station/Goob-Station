using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StationEvents;

[Prototype("incompatibleModes")]
public sealed class IncompatibleGameModesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public HashSet<string> Modes = new();
}
