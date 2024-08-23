using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Vehicles;

[RegisterComponent, NetworkedComponent]
public sealed partial class VehicleComponent : Component
{
    [ViewVariables]
    public EntityUid? Driver;

    [ViewVariables]
    public EntityUid? HornAction;

    [ViewVariables]
    public EntityUid? SirenAction;

    [ViewVariables]
    public bool FirstRun = true;

    public bool SirenEnabled = false;

    public EntityUid? SirenStream;

    /// <summary>
    /// If non-zero how many virtual items to spawn on the driver
    /// unbuckles them if they dont have enough
    /// </summary>
    [DataField]
    public int RequiredHands = 1;

    /// <summary>
    /// Will the vehicle move when a driver buckles
    /// </summary>
    [DataField]
    public bool EngineRunning = false;

    /// <summary>
    /// What sound to play when the driver presses the horn action (plays once)
    /// </summary>
    [DataField]
    public SoundSpecifier? HornSound;

    /// <summary>
    /// What sound to play when the driver presses the siren action (loops)
    /// </summary>
    [DataField]
    public SoundSpecifier? SirenSound;
}
[Serializable, NetSerializable]
public enum VehicleState : byte
{
    Animated,
    DrawOver
}
