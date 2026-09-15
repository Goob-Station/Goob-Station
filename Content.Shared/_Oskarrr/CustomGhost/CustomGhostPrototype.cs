// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Oskarrr.CustomGhost;

[DataDefinition]
[Prototype]
public sealed partial class CustomGhostPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("ckey", required: true)]
    public string Ckey { get; set; } = default!;

    [DataField("sprite", required: true)]
    public List<ResPath> CustomSpritePath { get; set; } = new();

    [DataField("alpha")]
    public float AlphaOverride { get; set; } = -1;

    /// <summary>Multiplier for the server-wide pixel limit; 0 disables it.</summary>
    [DataField("maxSize")]
    public float MaxSize { get; set; } = 1f;

    [DataField("ghostName")]
    public string GhostName = string.Empty;

    [DataField("ghostDescription")]
    public string GhostDescription = string.Empty;
}

[Serializable, NetSerializable]
public enum CustomGhostAppearance
{
    Sprite,
    AlphaOverride,
    MaxSize
}
