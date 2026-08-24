// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Effects;

/// <summary>
/// Raised on the server and sent to a client to play the color flash animation.
/// </summary>
[Serializable, NetSerializable]
public sealed class ColorFlashEffectEvent : EntityEventArgs
{
    /// <summary>
    /// Color to play for the flash.
    /// </summary>
    public Color Color;

    public List<NetEntity> Entities;

    /// <summary>
    /// The length of the flash animation.
    /// </summary>
    public float? AnimationLength;

    public ColorFlashEffectEvent(Color color, List<NetEntity> entities, float? animationLength = null)
    {
        Color = color;
        Entities = entities;
        AnimationLength = animationLength; // EE Plasmeme Change
    }
}