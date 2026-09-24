using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoiceRadioComponent : Component
{
    public override bool SendOnlyToOwner => true;

    [AutoNetworkedField]
    public List<ProtoId<RadioChannelPrototype>> Channels = new();

    [AutoNetworkedField]
    public ProtoId<RadioChannelPrototype>? Active;
}

[Serializable, NetSerializable]
public sealed class VoiceRadioSelectEvent(ProtoId<RadioChannelPrototype>? channel) : EntityEventArgs
{
    public readonly ProtoId<RadioChannelPrototype>? Channel = channel;
}
