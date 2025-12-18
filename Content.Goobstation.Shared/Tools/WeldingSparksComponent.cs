using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Tools;

/// <summary>
/// Creates an effect when a tool with this component is used on an entity.
/// </summary>
[RegisterComponent]
public sealed partial class WeldingSparksComponent : Component
{
    /// <summary>
    /// Prototype of the effect to spawn. (Defaults to <c>EffectWeldingSparks</c>)
    /// </summary>
    [DataField("effect")]
    public EntProtoId EffectProto = "EffectWeldingSparks";
}

/// <summary>
/// Raised by <c>WeldingSparksSystem</c> if the target has <see cref="Content.Shared.Doors.Components.DoorComponent"/>.
/// </summary>
/// <param name="sparksEnt">The welding sparks entity to animate.</param>
/// <param name="animData">Settings for the animation.</param>
/// <seealso cref="WeldAnimationData"/>
[Serializable, NetSerializable]
public sealed class PlayWeldAnimationEvent(NetEntity sparksEnt, WeldAnimationData animData) : EntityEventArgs
{
    public NetEntity SparksEnt = sparksEnt;
    public WeldAnimationData AnimData = animData;
}

/// <summary>
/// Enum specifiying the movement direction of the animation, to match up with the seam on the door.
/// </summary>
public enum AnimationDir : byte
{
    /// <summary>Weld top-to-bottom. (Airlocks)</summary>
    Vertical,
    /// <summary>Weld left-to-right. (Firelocks)</summary>
    Horizontal
}

/// <summary>
/// Various settings for the welding sparks animation, collected together in a record for ease of access.
/// </summary>
/// <param name="WeldDir">The direction the sparks effect should be animated in. (Currently vertical or horizontal)</param>
/// <param name="IsAlreadyWelded">Has the target already been welded shut. (If so, the animation plays backwards)</param>
/// <param name="Duration">How long should the animation take to complete.</param>
/// <seealso cref="PlayWeldAnimationEvent"/>
[Serializable, NetSerializable]
public readonly record struct WeldAnimationData(AnimationDir WeldDir, bool IsAlreadyWelded, TimeSpan Duration);
