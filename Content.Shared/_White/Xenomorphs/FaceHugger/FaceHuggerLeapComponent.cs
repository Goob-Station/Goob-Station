using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.FaceHugger;

/// <summary>
/// Handles the leap action for sentient facehuggers
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FaceHuggerLeapComponent : Component
{
    /// <summary>
    /// The leap action entity
    /// </summary>
    [ViewVariables]
    public EntityUid? LeapActionEntity;

    /// <summary>
    /// The leap action prototype ID
    /// </summary>
    [DataField]
    public EntProtoId LeapAction = "ActionFaceHuggerLeap";

    /// <summary>
    /// Speed of the leap
    /// </summary>
    [DataField]
    public float LeapSpeed = 6f;

    /// <summary>
    /// Sound played when leaping
    /// </summary>
    [DataField]
    public SoundSpecifier? LeapSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");

    /// <summary>
    /// Whether the facehugger is currently mid-leap
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsLeaping;
}
