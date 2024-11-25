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

    /// <summary>
    /// If they should be rendered ontop of the vehicle if true or behind
    /// </summary>
    [DataField]
    public VehicleRenderOver RenderOver = VehicleRenderOver.None;
}
[Serializable, NetSerializable]
public enum VehicleState : byte
{
    Animated,
    DrawOver
}

[Serializable, NetSerializable, Flags]
public enum VehicleRenderOver
{
    None = 0,
    North = 1,
    NorthEast = 2,
    East = 4,
    SouthEast = 8,
    South = 16,
    SouthWest = 32,
    West = 64,
    NorthWest = 128,
}
