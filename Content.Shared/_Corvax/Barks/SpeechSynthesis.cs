using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Corvax.Speech.Synthesis.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeechSynthesisComponent : Component
{
    [DataField("voice", customTypeSerializer: typeof(PrototypeIdSerializer<BarkPrototype>)), AutoNetworkedField]
    public string? VoicePrototypeId { get; set; }
}
