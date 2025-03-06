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

    /// <summary>
    /// not added to datafield. Used for remembering if its powered
    /// </summary>
    public bool IsPowered = false;

    /// <summary>
    ///  random chance for false triggering or not.
    /// number between 0 and 100
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int FalseDetectingChance = 5;

    /// <summary>
    ///  list of scanned entity and time scanned for scan timout
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<EntityUid, TimeSpan> Scanned = new Dictionary<EntityUid, TimeSpan>();

    /// <summary>
    ///  time in seconds for each scan of the entity to happen.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ScanTimeOut = 3f;

}
