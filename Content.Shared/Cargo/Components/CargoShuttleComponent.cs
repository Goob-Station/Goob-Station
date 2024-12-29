using Robust.Shared.GameStates;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Present on cargo shuttles to provide metadata such as preventing spam calling.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class CargoShuttleComponent : Component
{
    /*
     * Still needed for drone console for now.
     */
}
