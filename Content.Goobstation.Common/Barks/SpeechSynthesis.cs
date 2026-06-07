using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Barks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeechSynthesisComponent : Component
{
    [DataField("voice"), AutoNetworkedField]
    public ProtoId<BarkPrototype>? VoicePrototypeId;
}
