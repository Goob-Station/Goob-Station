using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.VoiceChat;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoiceChatCanadianComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MouthHeight = 8f;

    [DataField, AutoNetworkedField]
    public float HingeOffset = 4f;

    [DataField, AutoNetworkedField]
    public float MaxAngle = 35f;

    [DataField, AutoNetworkedField]
    public float MaxLift = 4f;

    [DataField, AutoNetworkedField]
    public float Sensitivity = 1.4f;

    [DataField, AutoNetworkedField]
    public float HingeLift = 1f;

    [DataField, AutoNetworkedField]
    public float PivotSmoothing = 0.15f;

    [DataField, AutoNetworkedField]
    public float MinPivotTime = 0.35f;

    [DataField, AutoNetworkedField]
    public float MaxPivotTime = 0.9f;
}
