using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Contraband;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
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
    /// Chance for false triggering.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FalseDetectingChance = 0.05f;

    /// <summary>
    /// Fake scanning when wire cut
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsFalseScanning = false;

    /// <summary>
    /// Increase false detecting chance when wire cut
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsFalseDetectingChanged = false;

    [DataField]
    public float FalseDetectingChanceMultiplier = 10f;

    /// <summary>
    ///  list of scanned entity and time scanned for scan timout
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<EntityUid, TimeSpan> Scanned = new Dictionary<EntityUid, TimeSpan>();

    /// <summary>
    ///  time in seconds for each scan of the entity to happen.
    /// </summary>
    [DataField]
    public TimeSpan ScanTimeOut = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Current detector state
    /// </summary>
    [DataField, AutoNetworkedField]
    public ContrabandDetectorState State = ContrabandDetectorState.Off;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan LastScanTime = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public enum ContrabandDetectorVisuals
{
    VisualState
}

[Serializable, NetSerializable]
public enum ContrabandDetectorState
{
    Off,
    Powered,
    Alarm,
    Scan
}

[Serializable, NetSerializable]
public enum ContrabandDetectorChanceWireKey : byte
{
    StatusKey,
    TimeoutKey
}

[Serializable, NetSerializable]
public enum ContrabandDetectorFakeScanWireKey : byte
{
    StatusKey,
    TimeoutKey
}
