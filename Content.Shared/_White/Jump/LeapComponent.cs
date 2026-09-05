using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Jump;

[RegisterComponent]
public sealed partial class LeapComponent : Component
{
    [DataField]
    public SoundSpecifier? JumpSound;

    [DataField]
    public float JumpSpeed = 7f;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId JumpAction = "ActionLeapXenomorph";

    [ViewVariables]
    public EntityUid? JumpActionEntity;
}
