using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Handles upgrading, boosting, ascension and other bongle related systems.
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
