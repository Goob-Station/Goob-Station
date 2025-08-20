using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.CustomGhostSystem;

/// <summary>
/// Use this for custom ghost's
/// </summary>
[Prototype("customGhost")]
public sealed class CustomGhostPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public string Ckey { get; } = default!;

    [DataField("sprite", required: true)]
    public ResPath CustomSpritePath { get; } = default!;

    [DataField("alpha")]
    public float AlphaOverride { get; } = -1;

    [DataField("ghostName")]
    public string GhostName = string.Empty;

    [DataField("ghostDescription")]
    public string GhostDescription = string.Empty;

    [DataField("size")]
    public Vector2 SizeOverride = Vector2.One;
}

[Serializable, NetSerializable]
public enum CustomGhostAppearance
{
    Sprite,
    AlphaOverride,
    SizeOverride
} 
