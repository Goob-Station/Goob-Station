using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoiceBroadcastConsoleComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(90);

    [AutoNetworkedField]
    public EntityUid? Broadcaster;

    [AutoNetworkedField]
    public TimeSpan EndTime;

    [AutoNetworkedField]
    public TimeSpan NextBroadcast;
}

[Serializable, NetSerializable]
public sealed class VoiceBroadcastToggleMessage : BoundUserInterfaceMessage;
