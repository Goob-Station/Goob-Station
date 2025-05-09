using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SpellCard;

/// <summary>
/// Data that is used for playing a spell card animation.
/// </summary>
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class SpellCardAnimationData
{
    /// <summary>
    /// The name of the spell card. If specified, will be displayed on the screen
    /// </summary>
    [DataField]
    public string? Name;

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
    /// Starting point for text to appear.
    /// </summary>
    [DataField]
    public Vector2 TextPosition;

    /// <summary>
    /// How much we should scale the text.
    /// </summary>
    [DataField]
    public float TextScale = 1f;

    /// <summary>
    /// Angle that the texture will move to.
    /// </summary>
    [DataField]
    public Angle MoveAngle;

    /// <summary>
    /// Speed of the texture, in pixels per second.
    /// </summary>
    [DataField]
    public float MoveSpeed = 70f;

    /// <summary>
    /// Entity to use for a sprite.
    /// Animation will fail to play if entity doesn't exist on client on its start.
    /// </summary>
    [ViewVariables]
    public NetEntity Source;

    /// <summary>
    /// Entity that is used to draw a sprite from. Copies all layers from the Source at
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
    /// How much opacity we add each second while in fade-in stage of animation.
    /// </summary>
    public float FadeInOpacityChange => MaxOpacity / FadeInDuration;

    /// <summary>
    /// How much opacity we remove each second while in fade-out stage of animation.
    /// </summary>
    public float FadeOutOpacityChange => MaxOpacity / FadeOutDuration;

    public static readonly SpellCardAnimationData DefaultAnimation = new()
    {
        Name = null,
        TotalDuration = 2.8f,
        Scale = 10f,
        MaxOpacity = 0.6f,
        FadeInDuration = 0.8f,
        FadeOutDuration = 0.8f,
        StartPosition = new Vector2(-300, -100),
        TextPosition = new Vector2(-100, 100),
        MoveAngle = new Angle(Math.PI * 1.5),
        MoveSpeed = 60,
    };

    public SpellCardAnimationData WithName(string name)
    {
        Name = name;
        return this;
    }

    public SpellCardAnimationData WithSource(NetEntity source)
    {
        Source = source;
        return this;
    }
}
