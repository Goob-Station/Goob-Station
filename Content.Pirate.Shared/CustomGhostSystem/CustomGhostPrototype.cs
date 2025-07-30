using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Pirate.Shared.CustomGhostSystem;

/// <summary>
/// This is a prototype for...
/// </summary>
[DataDefinition]
[Prototype("customGhost")]
public sealed partial class CustomGhostPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("ckey", required: true)]
    public string Ckey { get; set; } = default!;

    [DataField("sprite", required: true)]
    public List<ResPath> CustomSpritePath { get; set; } = new();

    [DataField("alpha")]
    public float AlphaOverride { get; set; } = -1;

    [DataField("ghostName")]
    public string GhostName = string.Empty;

    [DataField("ghostDescription")]
    public string GhostDescription = string.Empty;
}

[Serializable, NetSerializable]
public enum CustomGhostAppearance
{
    Sprite,
    AlphaOverride
}
