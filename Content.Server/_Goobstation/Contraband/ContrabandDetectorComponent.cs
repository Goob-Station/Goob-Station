using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Contraband;

/// <summary>
/// comonent added to contraband detector.
/// </summary>
[RegisterComponent]
public sealed partial class ContrabandDetectorComponent : Component
{
    /// <summary>
    /// Trigger sound effect when contraband is not found
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? NoDetect;
    /// <summary>
    /// Trigger sound effect when contraband is detected
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? Detect;

    public bool IsPowered = false;
}
