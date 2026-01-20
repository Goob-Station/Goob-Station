using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Emag.Prototypes;

/// <summary>
/// Emag type prototype definitions.
/// Used to define different emag types for various emagged effects.
/// </summary>
[Prototype ("emagType")]
public sealed partial class EmagTypePrototype : IPrototype
{
    [IdDataField] 
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Optional display name for UI purposes.
    /// </summary>
    [DataField]
    public string? Name { get; set; }
}
