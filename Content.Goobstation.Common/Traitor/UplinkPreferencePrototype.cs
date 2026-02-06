using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Traitor;

/// <summary>
/// Explains itself
/// </summary>
[Prototype]
public sealed partial class UplinkPreferencePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string? Loadout;

    // Null will force a fallback, which in our case is the implant
    [DataField]
    public string[]? SearchComponents;
}
