using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Jump;

[RegisterComponent]
public sealed partial class JumpComponent : Component
{
    [DataField]
    public SoundSpecifier? JumpSound;

    [DataField]
    public float JumpSpeed = 7f;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId JumpAction = "ActionJumpXenomorph";

    [ViewVariables]
    public EntityUid? JumpActionEntity;
}

public sealed partial class JumpActionEvent : WorldTargetActionEvent;
