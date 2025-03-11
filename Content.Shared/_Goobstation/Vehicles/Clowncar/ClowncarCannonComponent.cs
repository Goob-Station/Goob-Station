using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Vehicles.Clowncar;

/// <summary>
/// Dummy component to mark the clowncar gun and handle events raised on it
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedClowncarSystem))]
public sealed partial class ClowncarCannonComponent : Component
{
}
