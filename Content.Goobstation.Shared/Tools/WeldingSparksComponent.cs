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
public enum AnimationType : byte
{
    /// <summary>Vertical</summary>
    Airlock,
    /// <summary>Horizontal</summary>
    Firelock
}

/// <summary>
/// I'll fill this in later
/// </summary>
/// <param name="Type"></param>
/// <param name="IsAlreadyWelded"></param>
/// <param name="Duration"></param>
[Serializable, NetSerializable]
public readonly record struct WeldAnimationData(AnimationType Type, bool IsAlreadyWelded, TimeSpan Duration);
