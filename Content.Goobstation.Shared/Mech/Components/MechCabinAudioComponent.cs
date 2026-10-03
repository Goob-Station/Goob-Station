using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Only exists to give the actual pilot the audio comp. Technically we could
/// make this a generic "Givemechpilot" comp but I'm being lazy
/// </summary>
[RegisterComponent]
public sealed partial class MechCabinAudioComponent : Component
{
    [DataField]
    public ProtoId<AudioPresetPrototype> Preset = "MechCabin";
}
