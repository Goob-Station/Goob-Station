using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Granted to gang members so they can see gang role icons on other members, leaders, and lockers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowGangIconsComponent : Component;
