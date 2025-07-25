using Robust.Shared.GameStates;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Present on the star shuttle to provide metadata such as preventing spam calling.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class XithShuttleComponent : Component
{
    /*
     * Neccessary for DroneConsole to work on the remote control
     */
}
