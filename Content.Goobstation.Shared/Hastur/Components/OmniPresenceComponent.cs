using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class OmnipresenceComponent : Component
{
    /// <summary>
    /// The prototype ID for the clones.
    /// </summary>
    [DataField]
    public EntProtoId CloneProto = "MobHasturClone";

    /// <summary>
    /// The distance at which the clones spawn from the King.
    /// </summary>
    [DataField]
    public float CloneDistance;

    /// <summary>
    /// The stun range around the clones.
    /// </summary>
    [DataField]
    public float StunRange;

    /// <summary>
    /// The stun duration.
    /// </summary>
    [DataField]
    public float StunDuration;

    /// <summary>
    /// How long clones remain before despawning (seconds).
    /// </summary>
    [DataField]
    public float CloneLifetime;
}
