using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Footwear that plays a distinct wet step sound while it is wet.
/// Reuses the <see cref="Content.Shared._Pirate.Clothing.Events.PirateMakeFootstepSoundEvent"/>
/// inventory relay, gated on the shoe's <see cref="WettableComponent.Wetness"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetFootstepComponent : Component
{
    // TauCeti plays this single file through playsound() with vary=TRUE, which randomizes pitch by
    // rand(8,12)*0.1 (±20%) each step. Mirror that so repeated steps don't sound identical.
    [DataField]
    public SoundSpecifier Sound =
        new SoundPathSpecifier("/Audio/_Pirate/Effects/Footsteps/wet_shoes_step.ogg")
        {
            Params = AudioParams.Default.WithVariation(0.2f).WithVolume(-2f)
        };
}
