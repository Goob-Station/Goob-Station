using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Footwear that overrides steps while wet.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetFootstepComponent : Component
{
    [DataField]
    public SoundSpecifier Sound =
        new SoundPathSpecifier("/Audio/_Pirate/Effects/Footsteps/wet_shoes_step.ogg")
        {
            Params = AudioParams.Default.WithVariation(0.2f).WithVolume(-2f)
        };
}
