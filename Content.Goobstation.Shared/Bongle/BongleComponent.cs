using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Bongle
/// </summary>
[RegisterComponent]
public sealed partial class BongleComponent : Component
{
    [DataField]
    public bool Prime;

    [DataField]
    public EntProtoId? HillProto;

    /// <summary>
    /// New comps on ascension.
    /// </summary>
    [DataField]
    public ComponentRegistry AscendComponents;
}
