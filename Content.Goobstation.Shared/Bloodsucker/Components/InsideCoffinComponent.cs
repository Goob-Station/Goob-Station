using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Marks an entity as being inside a coffin. Mostly focused around the bloodsuckers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InsideCoffinComponent : Component;
