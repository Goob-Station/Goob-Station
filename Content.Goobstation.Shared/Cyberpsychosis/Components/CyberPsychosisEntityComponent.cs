using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CyberSanity;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberPsychosisEntityComponent : Component
{
    [ViewVariables]
    public EntityUid PsychosisOwner;

    [AutoNetworkedField]
    public EntityUid? TeleportAction;

    [AutoNetworkedField]
    public EntityUid? SpeechAction;

    [AutoNetworkedField]
    public EntityUid? OvertakeAction;
}
