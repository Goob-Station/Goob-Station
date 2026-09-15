using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.WallConversion;

[RegisterComponent]
public sealed partial class VoidwalkerWallConversionComponent : Component
{
    /// <summary>
    /// Structures with this tag are convertable.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> WallTag = "Wall";

    /// <summary>
    /// Structures that are converted are granted this tag.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> VoidedStructureTag = "VoidedStructure";

    /// <summary>
    /// How long does it take to convert a wall?
    /// </summary>
    [DataField]
    public TimeSpan WallConvertTime = TimeSpan.FromSeconds(5);
}
