using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Audio;

/// <summary>
/// Changes the Reverb of everything that has this.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AudioReverbComponent : Component
{
    [DataField]
    public ProtoId<AudioPresetPrototype> Preset = "Timestop";
}
