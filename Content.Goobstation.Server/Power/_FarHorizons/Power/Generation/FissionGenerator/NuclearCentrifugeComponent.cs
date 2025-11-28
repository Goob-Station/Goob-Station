using Robust.Shared.Audio;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/centrifuge.dm

[RegisterComponent]
public sealed partial class NuclearCentrifugeComponent : Component
{
    /// <summary>
    /// Processed fuel
    /// </summary>
    [DataField]
    public float ExtractedFuel = 0;

    /// <summary>
    /// Fuel left to be processed
    /// </summary>
    [DataField]
    public float FuelToExtract = 0;

    /// <summary>
    /// Flag indicating the centrifuge is running
    /// </summary>
    [DataField]
    public bool Processing = false;

    /// <summary>
    /// Sound played when loading an item into the centrifuge
    /// </summary>
    [DataField]
    public SoundPathSpecifier SoundLoad = new("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");

    /// <summary>
    /// Sound played while the centrifuge is processing
    /// </summary>
    [DataField]
    public SoundPathSpecifier SoundProcess = new("/Audio/Machines/spinning.ogg");

    /// <summary>
    /// Sound played when the centrifuge failed to create any plutonium
    /// </summary>
    [DataField]
    public SoundPathSpecifier SoundFail = new("/Audio/Machines/buzz-two.ogg");

    /// <summary>
    /// Sound played when the centrifuge creates plutonium
    /// </summary>
    [DataField]
    public SoundPathSpecifier SoundSucceed = new("/Audio/Machines/ding.ogg");

    [ViewVariables]
    public EntityUid? AudioProcess;
}