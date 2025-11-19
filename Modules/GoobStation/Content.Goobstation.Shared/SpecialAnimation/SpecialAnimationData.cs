// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SpecialAnimation;

/// <summary>
/// Data that is used for playing a spell card animation.
/// </summary>
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class SpecialAnimationData
{
    #region Basic Customization

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    [DataField]
    public float TotalDuration = 3f;

    /// <summary>
    /// How much does the sprite scale.
    /// </summary>
    [DataField]
    public float Scale = 1f;

    /// <summary>
    /// Opacity of the main sprite will be rendered for the animation.
    /// </summary>
    [DataField]
    public float MaxOpacity = 0.6f;

    /// <summary>
    /// How long does the fade-in lasts.
    /// </summary>
    [DataField]
    public float FadeInDuration = 0.8f;

    /// <summary>
    /// How long does the fade-out lasts.
    /// </summary>
    [DataField]
    public float FadeOutDuration = 0.8f;

    /// <summary>
    /// Starting point for the animation to appear.
    /// </summary>
    [DataField]
    public Vector2 StartPosition;

    /// <summary>
    /// Ending point where animation will move.
    /// </summary>
    [DataField]
    public Vector2 EndPosition;

    #endregion

    #region Text Customization

    /// <summary>
    /// Text for the animation. If specified, will be displayed on the screen
    /// </summary>
    [DataField]
    public string? Text;

    /// <summary>
    /// Text for the animation. If specified, will be displayed on the screen
    /// </summary>
    [DataField]
    public Color TextOverrideColor = Color.White;

    /// <summary>
    /// Starting point for text to appear.
    /// </summary>
    [DataField]
    public Vector2 TextPosition;

    /// <summary>
    /// Font for the text to use.
    /// </summary>
    [DataField]
    public string TextFontPath = "/Fonts/NotoSans/NotoSans-Regular.ttf";

    /// <summary>
    /// Text font size.
    /// </summary>
    [DataField]
    public int TextFontSize = 26;

    #endregion

    /// <summary>
    /// Entity to use for a sprite.
    /// Animation will fail to play if entity doesn't exist on client on its start.
    /// </summary>
    [ViewVariables]
    public NetEntity Source;

    /// <summary>
    /// Entity that is used to draw a sprite from. Copies SpriteComponent from
    /// the Source and then uses it, so it won't break when Source leaves our PVS range.
    /// </summary>
    [ViewVariables]
    [NonSerialized]
    public EntityUid? AnimationEntity;

    /// <summary>
    /// How long this animation has been playing for.
    /// </summary>
    [ViewVariables]
    public float AnimationPosition;

    /// <summary>
    /// Opacity of the main sprite will be rendered for the animation.
    /// </summary>
    [ViewVariables]
    public float Opacity;

    /// <summary>
    /// What offset do we currently have.
    /// </summary>
    [ViewVariables]
    public Vector2 Position = Vector2.Zero;

    /// <summary>
    /// When did the animation started.
    /// </summary>
    [ViewVariables]
    public TimeSpan StartTime;

    /// <summary>
    /// Last time when this animation was updated.
    /// </summary>
    [ViewVariables]
    public TimeSpan LastTime;

    /// <summary>
    /// Most basic animation to use. Use this if you're lazy and don't want to make anything special,
    /// otherwise use <see cref="SpecialAnimationPrototype"/>.
    /// </summary>
    public static readonly SpecialAnimationData DefaultAnimation = new()
    {
        TotalDuration = 2.8f,
        Scale = 15f,
        MaxOpacity = 0.6f,
        FadeInDuration = 0.8f,
        FadeOutDuration = 0.8f,
        StartPosition = new Vector2(-300, -150),
        EndPosition = new Vector2(-300, 250),

        Text = null,
        TextOverrideColor = Color.White,
        TextPosition = new Vector2(-250, 100),
        TextFontSize = 26,
        TextFontPath = "/Fonts/NotoSans/NotoSans-Bold.ttf", // Bald.
    };

    /// <summary>
    /// Adds some text to the animation.
    /// </summary>
    public SpecialAnimationData WithText(string text)
    {
        Text = text;
        return this;
    }

    /// <summary>
    /// Sets some sprite source to the animation.
    /// </summary>
    /// <remarks>
    /// Make sure that this entity is loaded in PVS
    /// for the client that this animation is addressed to.
    /// </remarks>
    public SpecialAnimationData WithSource(NetEntity source)
    {
        Source = source;
        return this;
    }
}
