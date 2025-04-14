using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.Borgs;

/// <summary>
///  Information relating to a borg's subtype. Should be purely cosmetic.
/// </summary>
[Prototype]
public sealed partial class BorgSubtypePrototype : IPrototype
{
    [IdDataField]
    public required string ID { get; set; }

    /// <summary>
    /// Prototype to display in the selection menu for the subtype.
    /// </summary>
    [DataField]
    public required EntProtoId DummyPrototype;

    /// <summary>
    /// The sprite path belonging to this particular subtype.
    /// </summary>
    [DataField]
    public required ResPath SpritePath;

    /// <summary>
    /// The parent borg type that the subtype will be shown under in the selection menu.
    /// </summary>
    [DataField]
    public required ProtoId<BorgTypePrototype> ParentBorgType = "generic";
}
