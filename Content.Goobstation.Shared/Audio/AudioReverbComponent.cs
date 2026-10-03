using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Audio;

/// <summary>
/// Changes the Reverb of everything that has this.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AudioReverbComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<AudioPresetPrototype> Preset = "Timestop";
}
